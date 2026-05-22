using Microsoft.Extensions.Logging;
using StateManagment.Entity;
using StateManagment.Models;

namespace StateManagment
{
    public class StateManager : IStateManager
    {
        private readonly IChangeHandler changeHandler;
        private readonly IOrchestrator orchestrator;
        private readonly ICustomerDatabase dataStore;
        private readonly ILogger<StateManager> logger;

        public StateManager(IChangeHandler changeHandler, IOrchestrator orchestrator, ICustomerDatabase dataStore, ILogger<StateManager> logger)
        {
            this.orchestrator = orchestrator;
            this.dataStore = dataStore;
            this.logger = logger;
            this.changeHandler = changeHandler;
        }

        public async Task<TaskOutcome> Evaluate<T>(MessageEnvelop envelop) where T : IEntity
        {
            logger.LogInformation($"Evaluating change for Action='{envelop.Change}' EntityName='{envelop.Name}' CustomerId='{envelop.CustomerId}' EntityId='{envelop.EntityId}'");

            var latestEntity = await dataStore.FindEntity<T>(envelop.SearchBy()); 

            if (latestEntity == MessageEnvelop.NONE) { return TaskOutcome.NOT_FOUND; }

            logger.LogInformation($"EntityName='{envelop.Name}' CustomerId='{envelop.CustomerId}' EntityId='{envelop.EntityId}' is '{latestEntity.SubmittedVersion}' submitting for processing with RuntimeStatus='{RuntimeStatus.INITIATE}'");
            return await ProcessUpdateAsync<T>(new OrchestrationEnvelop
            {
                EntityId = envelop.EntityId,
                CustomerId = envelop.CustomerId,
                Name = envelop.Name,
                SubmittedVersion = latestEntity.SubmittedVersion,
                Status = RuntimeStatus.INITIATE
            });
        }

        /// <summary>
        /// The logic to decide whether the change can be processed based on the current state of 
        /// the entity and the status of the orchestration.
        /// </summary>
        public async Task<TaskOutcome> ProcessUpdateAsync<T>(OrchestrationEnvelop operationalEntity) where T : IEntity
        {
            try
            {
                logger.LogInformation($"Starting StateManager Processing update for EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}' with RuntimeStatus='{operationalEntity.Status}'");

                logger.LogInformation($"Requesting lock for EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}'");
                var entityLockResult = await changeHandler.TakeEntityLock(operationalEntity.EntityId);
                if (!entityLockResult.Successful) return entityLockResult;
                
                var storedEntity = await dataStore.GetBasicInfo<T>(operationalEntity.SearchBy());
                var submittedVersionCompare = storedEntity.SubmittedVersion == operationalEntity.SubmittedVersion;

                // If processed storedEntity version is different from the submitted version, then we have to re-evaluate the change. This is to make sure that we are not missing any changes which were submitted while the orchestration was in progress. If versions are same, then we can continue with the orchestration as is.
                if (!submittedVersionCompare && storedEntity.SubmittedVersion > operationalEntity.SubmittedVersion)
                {
                    logger.LogWarning($"EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}' has newer version '{storedEntity.SubmittedVersion}' than the submitted version '{operationalEntity.SubmittedVersion}'. Restarting evaluation with the latest version.");
                    await changeHandler.ChangeStatusTo<T>(operationalEntity.SearchBy(), EntityState.EVALUATION_RESTARTING);

                    return await orchestrator.EvaluateAsync(operationalEntity.EntityId, operationalEntity.Name);
                }

                // There is a scenario where the stored entity version is lower than the operational entity version. This can happen when there are multiple updates happening in a quick succession and the orchestration is taking longer time to process the changes. In this case, we can either choose to fail the orchestration or we can choose to continue with the orchestration as is. For now, we will choose to continue with the orchestration as is, but we can consider failing the orchestration in future if this becomes a common scenario.

                switch (storedEntity.State)
                {
                    case EntityState.NEW when operationalEntity.Status == RuntimeStatus.INITIATE:
                    case EntityState.IN_REVIEW when operationalEntity.Status == RuntimeStatus.INITIATE:
                    case EntityState.ATTENTION_REQUIRED when operationalEntity.Status == RuntimeStatus.INITIATE:
                    case EntityState.SYNCHRONISED when operationalEntity.Status == RuntimeStatus.INITIATE:
                        logger.LogInformation($"EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}' is starting evaluation with RuntimeStatus='{operationalEntity.Status}'. Requesting Evaluate Step to start.");
                        await orchestrator.EvaluateAsync(operationalEntity.EntityId, operationalEntity.Name);
                        break;

                    case EntityState.NEW when operationalEntity.Status == RuntimeStatus.EVALUATION_STARTED:
                    case EntityState.IN_REVIEW when operationalEntity.Status == RuntimeStatus.EVALUATION_STARTED:
                    case EntityState.ATTENTION_REQUIRED when operationalEntity.Status == RuntimeStatus.EVALUATION_STARTED:
                    case EntityState.SYNCHRONISED when operationalEntity.Status == RuntimeStatus.EVALUATION_STARTED:
                    case EntityState.EVALUATION_RESTARTING when operationalEntity.Status == RuntimeStatus.EVALUATION_STARTED:
                        logger.LogInformation($"EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}' is in '{storedEntity.State}' state and received RuntimeStatus='{operationalEntity.Status}'. Changing state to '{EntityState.EVALUATING}' and continuing evaluation.");
                        await changeHandler.ChangeStatusTo<T>(operationalEntity.SearchBy(), EntityState.EVALUATING, operationalEntity.Feedbacks, operationalEntity.OrchestrationData);
                        break;

                    case EntityState.EVALUATING when operationalEntity.Status == RuntimeStatus.EVALUATION_COMPLETED:
                        await changeHandler.ChangeStatusTo<T>(operationalEntity.SearchBy(), EntityState.IN_PROGRESS, operationalEntity.Feedbacks, operationalEntity.OrchestrationData);
                        logger.LogInformation($"EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}' has completed evaluation. Requesting Apply Step to start with RuntimeStatus='{operationalEntity.Status}'.");
                        await orchestrator.ApplyAsync(operationalEntity.EntityId, operationalEntity.Name);

                        break;
                    case EntityState.EVALUATING when operationalEntity.Status == RuntimeStatus.EVALUATION_INCOMPLETE:
                    case EntityState.IN_REVIEW when operationalEntity.Status == RuntimeStatus.EVALUATION_INCOMPLETE:
                    case EntityState.IN_PROGRESS when operationalEntity.Status == RuntimeStatus.CHANGE_FAILED:
                        logger.LogInformation($"EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}' has incomplete evaluation or change failed. Changing state to '{EntityState.ATTENTION_REQUIRED}' and requesting manual review.");
                        await changeHandler.ChangeStatusTo<T>(operationalEntity.SearchBy(), EntityState.ATTENTION_REQUIRED, operationalEntity.Feedbacks, operationalEntity.OrchestrationData);

                        break;
                    case EntityState.EVALUATING when operationalEntity.Status == RuntimeStatus.EVALUATION_REQUIRES_MANUAL_REVIEW:
                        logger.LogInformation($"EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}' requires manual review. Changing state to '{EntityState.IN_REVIEW}' and requesting manual review.");
                        await changeHandler.ChangeStatusTo<T>(operationalEntity.SearchBy(), EntityState.IN_REVIEW, operationalEntity.Feedbacks, operationalEntity.OrchestrationData);
                        break;

                    case EntityState.IN_PROGRESS when operationalEntity.Status == RuntimeStatus.CHANGE_APPLIED:
                        logger.LogInformation($"EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}' has applied change. Changing state to '{EntityState.SYNCHRONISED}' and completing orchestration.");
                        await changeHandler.ChangeStatusTo<T>(operationalEntity.SearchBy(), EntityState.SYNCHRONISED, operationalEntity.Feedbacks, operationalEntity.OrchestrationData);
                        await orchestrator.PostApplyAsync(operationalEntity.EntityId, operationalEntity.Name);
                        break;

                    case EntityState.IN_PROGRESS when operationalEntity.Status == RuntimeStatus.CHANGE_EXTERNAL:
                        logger.LogInformation($"EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}' has detected external change. Changing state to '{EntityState.IN_PROGRESS_EXTERNAL}' and requesting orchestration to handle external change.");
                        await changeHandler.ChangeStatusTo<T>(operationalEntity.SearchBy(), EntityState.IN_PROGRESS_EXTERNAL, operationalEntity.Feedbacks, operationalEntity.OrchestrationData);
                        break;

                    case EntityState.IN_PROGRESS_EXTERNAL when operationalEntity.Status == RuntimeStatus.CHANGE_EXTERNAL_COMPLETED:
                        logger.LogInformation($"EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}' has completed handling external change. Changing state to '{EntityState.SYNCHRONISED}' and completing orchestration.");
                        await changeHandler.ChangeStatusTo<T>(operationalEntity.SearchBy(), EntityState.SYNCHRONISED, operationalEntity.Feedbacks, operationalEntity.OrchestrationData);
                        await orchestrator.PostApplyAsync(operationalEntity.EntityId, operationalEntity.Name);
                        break;

                    default:
                        logger.LogWarning($"EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}' with RuntimeStatus='{operationalEntity.Status}' is not expected to be in state '{storedEntity.State}'. This indicates a possible issue in the orchestration logic or an unexpected update. Please investigate the orchestration flow and the updates being submitted for this entity.");
                        return TaskOutcome.TRANSITION_NOT_SUPPORTED;
                }

            }
            finally
            {
                logger.LogInformation($"Releasing lock for EntityName='{operationalEntity.Name}' CustomerId='{operationalEntity.CustomerId}' EntityId='{operationalEntity.EntityId}'");
                await changeHandler.ReleaseEntityLock(operationalEntity.EntityId);
            }

            return TaskOutcome.OK;
        }
    }
}

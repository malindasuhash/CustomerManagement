using StateManagment.Models;

namespace StateManagment
{
    public class StateManager : IStateManager
    {
        private readonly IChangeHandler changeHandler;
        private readonly IOrchestrator orchestrator;
        private readonly ICustomerDatabase dataRetriever;

        public StateManager(IChangeHandler changeHandler, IOrchestrator orchestrator, ICustomerDatabase dataStore)
        {
            this.orchestrator = orchestrator;
            this.dataRetriever = dataStore;
            this.changeHandler = changeHandler;
        }

        /// <summary>
        /// The logic to decide whether the change can be processed based on the current state of 
        /// the entity and the status of the orchestration.
        /// </summary>
        public async Task<TaskOutcome> ProcessUpdateAsync(OrchestrationEnvelop operationalEntity)
        {
            try
            {
                var entityLockResult = await changeHandler.TakeEntityLock(operationalEntity.EntityId);
                if (!entityLockResult.Successful) return entityLockResult;

                var storedEntity = dataRetriever.GetBasicInfo(operationalEntity.Name, operationalEntity.EntityId);
                var submittedVersionCompare = storedEntity.SubmittedVersion == operationalEntity.SubmittedVersion;

                // If processed storedEntity version is different from the submitted version, then we have to re-evaluate the change. This is to make sure that we are not missing any changes which were submitted while the orchestration was in progress. If versions are same, then we can continue with the orchestration as is.
                if (!submittedVersionCompare && storedEntity.SubmittedVersion > operationalEntity.SubmittedVersion)
                {
                    await changeHandler.ChangeStatusTo(operationalEntity.EntityId, operationalEntity.Name, EntityState.EVALUATION_RESTARTING);

                    return await orchestrator.EvaluateAsync(operationalEntity.EntityId, operationalEntity.Name);
                }

                // There is a scenario where the stored entity version is lower than the operational entity version. This can happen when there are multiple updates happening in a quick succession and the orchestration is taking longer time to process the changes. In this case, we can either choose to fail the orchestration or we can choose to continue with the orchestration as is. For now, we will choose to continue with the orchestration as is, but we can consider failing the orchestration in future if this becomes a common scenario.

                switch (storedEntity.State)
                {
                    case EntityState.NEW when operationalEntity.Status == RuntimeStatus.INITIATE:
                    case EntityState.IN_REVIEW when operationalEntity.Status == RuntimeStatus.INITIATE:
                    case EntityState.ATTENTION_REQUIRED when operationalEntity.Status == RuntimeStatus.INITIATE:
                    case EntityState.SYNCHRONISED when operationalEntity.Status == RuntimeStatus.INITIATE:
                        await orchestrator.EvaluateAsync(operationalEntity.EntityId, operationalEntity.Name);
                        break;

                    case EntityState.NEW when operationalEntity.Status == RuntimeStatus.EVALUATION_STARTED:
                    case EntityState.IN_REVIEW when operationalEntity.Status == RuntimeStatus.EVALUATION_STARTED:
                    case EntityState.ATTENTION_REQUIRED when operationalEntity.Status == RuntimeStatus.EVALUATION_STARTED:
                    case EntityState.SYNCHRONISED when operationalEntity.Status == RuntimeStatus.EVALUATION_STARTED:
                    case EntityState.EVALUATION_RESTARTING when operationalEntity.Status == RuntimeStatus.EVALUATION_STARTED:
                        await changeHandler.ChangeStatusTo(operationalEntity.EntityId, operationalEntity.Name, EntityState.EVALUATING, operationalEntity.Messages);
                        break;

                    case EntityState.EVALUATING when operationalEntity.Status == RuntimeStatus.EVALUATION_COMPLETED:
                        await changeHandler.ChangeStatusTo(operationalEntity.EntityId, operationalEntity.Name, EntityState.IN_PROGRESS, operationalEntity.Messages);
                        await orchestrator.ApplyAsync(operationalEntity.EntityId, operationalEntity.Name);

                        break;
                    case EntityState.EVALUATING when operationalEntity.Status == RuntimeStatus.EVALUATION_INCOMPLETE:
                    case EntityState.IN_REVIEW when operationalEntity.Status == RuntimeStatus.EVALUATION_INCOMPLETE:
                    case EntityState.IN_PROGRESS when operationalEntity.Status == RuntimeStatus.CHANGE_FAILED:
                        await changeHandler.ChangeStatusTo(operationalEntity.EntityId, operationalEntity.Name, EntityState.ATTENTION_REQUIRED, operationalEntity.Messages);

                        break;
                    case EntityState.EVALUATING when operationalEntity.Status == RuntimeStatus.EVALUATION_REQUIRES_MANUAL_REVIEW:
                        await changeHandler.ChangeStatusTo(operationalEntity.EntityId, operationalEntity.Name, EntityState.IN_REVIEW, operationalEntity.Messages);
                        break;

                    case EntityState.IN_PROGRESS when operationalEntity.Status == RuntimeStatus.CHANGE_APPLIED:
                        await changeHandler.ChangeStatusTo(operationalEntity.EntityId, operationalEntity.Name, EntityState.SYNCHRONISED, operationalEntity.Messages);
                        await orchestrator.PostApplyAsync(operationalEntity.EntityId, operationalEntity.Name);
                        break;

                    default:
                        return TaskOutcome.TRANSITION_NOT_SUPPORTED;
                }

            }
            finally
            {
                await changeHandler.ReleaseEntityLock(operationalEntity.EntityId);
            }

            return TaskOutcome.OK;
        }
    }
}

using StateManager.Events;
using StateManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager
{
    public class StateManager : IStateManager
    {
        private readonly IChangeHandler changeHandler;
        private readonly IOrchestrator orchestrator;
        private readonly IDataRetriever dataRetriever;

        public StateManager(IChangeHandler changeHandler, IOrchestrator orchestrator, IDataRetriever dataRetriever)
        {
            this.orchestrator = orchestrator;
            this.dataRetriever = dataRetriever;
            this.changeHandler = changeHandler;
        }

        public async Task<TaskOutcome> ProcessUpdateAsync(OrchestrationEnvelop orchestrationEnvelop)
        {
            try
            {
                var entityLockResult = await changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId);
                if (!entityLockResult.Successful) return entityLockResult;

                var entity = await dataRetriever.GetEntityBasics(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
                var submittedVersionCompare = entity.SubmittedVersion == orchestrationEnvelop.SubmittedVersion;

                switch (entity.State)
                {
                    case EntityState.NEW when orchestrationEnvelop.Status == RuntimeStatus.INITIATE:
                    case EntityState.IN_REVIEW when orchestrationEnvelop.Status == RuntimeStatus.INITIATE:
                    case EntityState.ATTENTION_REQUIRED when orchestrationEnvelop.Status == RuntimeStatus.INITIATE:
                    case EntityState.SYNCHONISED when orchestrationEnvelop.Status == RuntimeStatus.INITIATE:
                        if (submittedVersionCompare)
                        {
                            var entityEnvelop = await dataRetriever.GetEntityEnvelop(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
                            await orchestrator.EvaluateAsync(entityEnvelop);
                        }
                        break;
                    case EntityState.NEW when orchestrationEnvelop.Status == RuntimeStatus.EVALUATION_STARTED:
                    case EntityState.IN_REVIEW when orchestrationEnvelop.Status == RuntimeStatus.EVALUATION_STARTED:
                    case EntityState.ATTENTION_REQUIRED when orchestrationEnvelop.Status == RuntimeStatus.EVALUATION_STARTED:
                    case EntityState.SYNCHONISED when orchestrationEnvelop.Status == RuntimeStatus.EVALUATION_STARTED:
                        if (submittedVersionCompare)
                        {
                            await changeHandler.ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.EVALUATING, orchestrationEnvelop.Messages);
                        }
                        if (entity.SubmittedVersion > orchestrationEnvelop.SubmittedVersion)
                        {
                            await changeHandler.ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.EVALUATION_RESTARTING);

                            var entityEnvelop = await dataRetriever.GetEntityEnvelop(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);

                            await orchestrator.EvaluateAsync(entityEnvelop);
                        }

                        break;
                    case EntityState.EVALUATING when orchestrationEnvelop.Status == RuntimeStatus.EVALUATION_COMPLETED:
                        if (submittedVersionCompare)
                        {
                            await changeHandler.ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.IN_PROGRESS, orchestrationEnvelop.Messages);
                            var entityEnvelop = await dataRetriever.GetEntityEnvelop(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
                            await orchestrator.ApplyAsync(entityEnvelop);
                        }
                        break;
                    case EntityState.EVALUATING when orchestrationEnvelop.Status == RuntimeStatus.EVALUATION_INCOMPLETE:
                    case EntityState.IN_REVIEW when orchestrationEnvelop.Status == RuntimeStatus.EVALUATION_INCOMPLETE:
                    case EntityState.IN_PROGRESS when orchestrationEnvelop.Status == RuntimeStatus.CHANGE_FAILED:
                        if (submittedVersionCompare)
                        {
                            await changeHandler.ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.ATTENTION_REQUIRED, orchestrationEnvelop.Messages);
                        }
                        break;
                    case EntityState.EVALUATING when orchestrationEnvelop.Status == RuntimeStatus.EVALUATION_REQUIRES_MANUAL_REVIEW:
                        if (submittedVersionCompare)
                        {
                            await changeHandler.ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.IN_REVIEW, orchestrationEnvelop.Messages);
                        }
                        break;
                    case EntityState.IN_PROGRESS when orchestrationEnvelop.Status == RuntimeStatus.CHANGE_APPLIED:
                        if (submittedVersionCompare)
                        {
                            await changeHandler.ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.SYNCHONISED, orchestrationEnvelop.Messages);
                            var entityEnvelop = await dataRetriever.GetEntityEnvelop(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
                            await orchestrator.PostApplyAsync(entityEnvelop);
                        }
                        break;
                    default:
                        return TaskOutcome.TRANSITION_NOT_SUPPORTED;
                }

            }
            finally
            {
                await changeHandler.ReleaseEntityLock(orchestrationEnvelop.EntityId);
            }

            return TaskOutcome.OK;
        }
    }
}

using StateManager.Events;
using StateManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager
{
    public class StateManager
    {
        private readonly IChangeHandler changeHandler;
        private readonly IOrchestrator orchestrator;
        private readonly IDataRetriever dataRetriever;

        public StateManager(IChangeHandler changeHandler, IOrchestrator orchestrator, IDataRetriever dataRetriever = null) 
        {
            this.orchestrator = orchestrator;
            this.dataRetriever = dataRetriever;
            this.changeHandler = changeHandler;
        }

        public Task<TaskOutcome> ProcessChangeAsync(MessageEnvelop envelop)
        {
            // When change is created, add to repository
            if (envelop.Change == ChangeType.Create && !envelop.Submitted)
            {
                changeHandler.Draft(envelop);
                return Task.FromResult(TaskOutcome.OK);
            }

            if (envelop.Change == ChangeType.Create && envelop.Submitted)
            {
                changeHandler.Draft(envelop);
                changeHandler.Submitted(envelop);
                var result = ProcessUpdateAsync(new OrchestrationEnvelop 
                { 
                    EntityId = envelop.EntityId, 
                    Name = envelop.Name, 
                    DraftVersion = envelop.DraftVersion, 
                    SubmittedVersion = envelop.SubmittedVersion, 
                    Status = RuntimeStatus.INITIATE
                });

                return result;
            }

            if (envelop.Change == ChangeType.Update && !envelop.Submitted)
            {
                changeHandler.TryDraft(envelop, out TaskOutcome outcome);
                return Task.FromResult(outcome);
            }

            if (envelop.Change == ChangeType.Update && envelop.Submitted)
            {
                if (changeHandler.TryDraft(envelop, out TaskOutcome outcome))
                {
                    // Consider what will happen if someone is taking a copy of submitted version
                    // whilst I am trying to update it. Should I ask for a lock at this point?
                    // If cannot take the lock, should I error out?
                    changeHandler.TryLockSubmitted(envelop, out TaskOutcome submitOutcome);
                    orchestrator.EvaluateAsync(envelop); // Start evalutation

                    return Task.FromResult(submitOutcome);
                }

                return Task.FromResult(outcome);
            }

            return Task.FromResult(TaskOutcome.OK);
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
                        if (submittedVersionCompare)
                        {
                            var entityEnvelop = await dataRetriever.GetEntityEnvelop(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
                            await orchestrator.EvaluateAsync(entityEnvelop);
                        }
                        break;
                    case EntityState.NEW when orchestrationEnvelop.Status == RuntimeStatus.EVALUATION_STARTED:
                        if (submittedVersionCompare)
                        {
                            await changeHandler.ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.EVALUATING);
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
                            await changeHandler.ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.IN_PROGRESS);
                            var entityEnvelop = await dataRetriever.GetEntityEnvelop(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
                            await orchestrator.ApplyAsync(entityEnvelop);
                        }
                        break;
                    case EntityState.EVALUATING when orchestrationEnvelop.Status == RuntimeStatus.EVALUATION_INCOMPLETE:
                        if (submittedVersionCompare)
                        {
                            await changeHandler.ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.ATTENTION_REQUIRED, orchestrationEnvelop.Messages);
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

using StateManager.Models;
using StateManagment.Models;

namespace StateManager
{
    public class ChangeProcessor
    {
        private readonly IChangeHandler changeHandler;
        private readonly IOrchestrator orchestrator;
        private readonly IStateManager stateManager;

        public ChangeProcessor(IChangeHandler changeHandler, IOrchestrator orchestrator, IStateManager stateManager)
        {
            this.orchestrator = orchestrator;
            this.stateManager = stateManager;
            this.changeHandler = changeHandler;
        }

        public async Task<TaskOutcome> ProcessChangeAsync(MessageEnvelop envelop)
        {
            // When change is created, add to repository
            if (envelop.Change == ChangeType.Create && !envelop.Submitted)
            {
                changeHandler.Draft(envelop);
                return TaskOutcome.OK;
            }

            if (envelop.Change == ChangeType.Create && envelop.Submitted)
            {
                changeHandler.Draft(envelop);
                changeHandler.Submitted(envelop);
                var result = await stateManager.ProcessUpdateAsync(new OrchestrationEnvelop
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
                var outcome = await changeHandler.TryDraft(envelop);
                return outcome;
            }

            if (envelop.Change == ChangeType.Update && envelop.Submitted)
            {
                var lockResult = await changeHandler.TryDraft(envelop);
                if (lockResult == TaskOutcome.OK)
                {
                    // Consider what will happen if someone is taking a copy of submitted version
                    // whilst I am trying to update it. Should I ask for a lock at this point?
                    // If cannot take the lock, should I error out?
                    var submitOutcome = await changeHandler.TryLockSubmitted(envelop);
                    if (submitOutcome != TaskOutcome.OK)
                    {
                        return submitOutcome;
                    }

                    var result = await stateManager.ProcessUpdateAsync(new OrchestrationEnvelop
                    {
                        EntityId = envelop.EntityId,
                        Name = envelop.Name,
                        DraftVersion = envelop.DraftVersion,
                        SubmittedVersion = envelop.SubmittedVersion,
                        Status = RuntimeStatus.INITIATE
                    });

                    return result;
                }

                return lockResult;
            }

           return TaskOutcome.CHANGE_NOT_SUPPORTED;
        }
    }
}

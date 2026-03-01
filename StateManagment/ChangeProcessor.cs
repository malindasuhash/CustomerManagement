using StateManagment.Models;

namespace StateManagment
{
    public class ChangeProcessor : IChangeProcessor
    {
        private readonly IChangeHandler changeHandler;
        private readonly IStateManager stateManager;
        private readonly IAuditManager auditManager;

        public ChangeProcessor(IChangeHandler changeHandler, IStateManager stateManager, IAuditManager auditManager)
        {
            this.stateManager = stateManager;
            this.auditManager = auditManager;
            this.changeHandler = changeHandler;
        }

        /// <summary>
        /// Processes the change based on the type of change and whether it is submitted or not.
        /// It handles the creation and updating of changes, as well as the submission process.
        /// </summary>
        public async Task<TaskOutcome> ProcessChangeAsync(MessageEnvelop envelop)
        {
            if (envelop.Change == ChangeType.Touch)
            {
                return await stateManager.Initiate(envelop.Name, envelop.EntityId);
            }

            if (envelop.Change == ChangeType.Submit)
            {
                var lockedResult = await changeHandler.TryLockSubmitted(envelop);

                if (lockedResult != TaskOutcome.OK)
                {
                    return lockedResult;
                }

                return await stateManager.Initiate(envelop.Name, envelop.EntityId);
            }

            if (envelop.Change == ChangeType.Delete)
            {
                await changeHandler.TryMergeDraft(envelop);

                if (envelop.IsSubmitted)
                {
                    var lockedResult = await changeHandler.TryLockSubmitted(envelop);
                    if (lockedResult != TaskOutcome.OK)
                    {
                        return lockedResult;
                    }

                    return await stateManager.Initiate(envelop.Name, envelop.EntityId);
                }

                return TaskOutcome.OK;
            }

            if (envelop.Change == ChangeType.Create)
            {
                await changeHandler.Draft(envelop);
                await auditManager.Write(envelop);

                if (envelop.IsSubmitted)
                {
                    changeHandler.Submitted(envelop);
                    var result = await stateManager.Initiate(envelop.Name, envelop.EntityId);
                    return result;
                }

                return TaskOutcome.OK;
            }

            if (envelop.Change == ChangeType.Update)
            {
                var outcome = await changeHandler.TryMergeDraft(envelop);

                if (envelop.IsSubmitted)
                {
                    if (outcome == TaskOutcome.OK)
                    {
                        // Consider what will happen if someone is taking a copy of submitted version
                        // whilst I am trying to update it. Should I ask for a lock at this point?
                        // If cannot take the lock, should I error out?
                        var submitOutcome = await changeHandler.TryLockSubmitted(envelop);
                        if (submitOutcome != TaskOutcome.OK)
                        {
                            return submitOutcome;
                        }

                        var result = await stateManager.Initiate(envelop.Name, envelop.EntityId);

                        return result;
                    }
                }
            }

            return TaskOutcome.CHANGE_NOT_SUPPORTED;
        }
    }
}

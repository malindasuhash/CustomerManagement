using Microsoft.Extensions.Logging;
using StateManagment.Entity;
using StateManagment.Models;

namespace StateManagment
{
    public class ChangeProcessor : IChangeProcessor
    {
        private readonly IChangeHandler changeHandler;
        private readonly IStateManager stateManager;
        private readonly ILogger<ChangeHandler> logger;

        public ChangeProcessor(IChangeHandler changeHandler, IStateManager stateManager, ILogger<ChangeHandler> logger)
        {
            this.stateManager = stateManager;
            this.logger = logger;
            this.changeHandler = changeHandler;
        }

        /// <summary>
        /// Processes the change based on the type of change and whether it is submitted or not.
        /// It handles the creation and updating of changes, as well as the submission process.
        /// </summary>
        public async Task<TaskOutcome> ProcessChangeAsync<T>(MessageEnvelop envelop) where T : IEntity
        {
            logger.LogInformation($"Processing change for Action='{envelop.Change}' EntityName='{envelop.Name}' CustomerId='{envelop.CustomerId}' EntityId='{envelop.EntityId}'");

            if (envelop.Change == ChangeType.Touch)
            {
                return await stateManager.Evaluate<T>(envelop);
            }

            if (envelop.Change == ChangeType.Submit)
            {
                var lockedResult = await changeHandler.TryLockSubmitted<T>(envelop);

                if (lockedResult != TaskOutcome.OK)
                {
                    return lockedResult;
                }

                return await stateManager.Evaluate<T>(envelop);
            }

            if (envelop.Change == ChangeType.Delete)
            {
                await changeHandler.TryMarkForRemoval<T>(envelop);

                if (envelop.IsSubmitted)
                {
                    return await stateManager.Evaluate<T>(envelop);
                }

                return TaskOutcome.OK;
            }

            if (envelop.Change == ChangeType.Create)
            {
                await changeHandler.Draft<T>(envelop);

                if (envelop.IsSubmitted)
                {
                    await changeHandler.Submitted<T>(envelop);
                    var result = await stateManager.Evaluate<T>(envelop);
                    return result;
                }

                return TaskOutcome.OK;
            }

            if (envelop.Change == ChangeType.Update)
            {
                logger.LogInformation($"Attempting to merge draft for CustomerId='{envelop.CustomerId}', EntityName='{envelop.Name}' EntityId='{envelop.EntityId}'");
                var outcome = await changeHandler.TryMergeDraft<T>(envelop);

                if (outcome != TaskOutcome.OK)
                {
                    logger.LogWarning($"Failed to merge draft for CustomerId='{envelop.CustomerId}', EntityName='{envelop.Name}' EntityId='{envelop.EntityId}' Outcome='{outcome.Reason}'. May need to retry.");
                    return outcome;
                }

                if (envelop.IsSubmitted)
                {
                    if (outcome == TaskOutcome.OK)
                    {
                        // Consider what will happen if someone is taking a copy of submitted version
                        // whilst I am trying to update it. Should I ask for a lock at this point?
                        // If cannot take the lock, should I error out?
                        var submitOutcome = await changeHandler.TryLockSubmitted<T>(envelop);
                        if (submitOutcome != TaskOutcome.OK)
                        {
                            logger.LogWarning($"Failed to lock submitted version for update for CustomerId='{envelop.CustomerId}', EntityName='{envelop.Name}' EntityId='{envelop.EntityId}' Outcome='{submitOutcome.Reason}'. May need to retry.");
                            return submitOutcome;
                        }

                        var result = await stateManager.Evaluate<T>(envelop);

                        return result;
                    }
                }

                return outcome;
            }

            logger.LogWarning($"Unsupported change type= '{envelop.Change}' for CustomerId='{envelop.CustomerId}', EntityName='{envelop.Name}' EntityId='{envelop.EntityId}'");

            return TaskOutcome.CHANGE_NOT_SUPPORTED;
        }
    }
}

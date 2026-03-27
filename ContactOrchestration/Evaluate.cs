using ContactOrchestration.Checks;
using StateManagment.Models;

namespace ContactOrchestration
{
    public class Evaluate
    {
        private readonly ISender sender;

        public Evaluate(ISender sender)
        {
            this.sender = sender;
        }

        public async Task Run(OrchestrationInfo runtimeInfo)
        {
            // Send EVALUATION_STARTED event
            await sender.SendAsync(OrchestrationEnvelop.Create(EntityName.Contact, runtimeInfo.EntityId, runtimeInfo.CustomerId, runtimeInfo.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED, null, null, runtimeInfo.LegalEntityId), runtimeInfo.CorellationId);

            // Marker in the Ochestration data to skip certain checks perhaps.
            if (runtimeInfo.OrchestrationData.Any(a => a.Key.Equals("SKIP_EVALUTE_RUN_APPLY")))
            {
                await sender.SendAsync(OrchestrationEnvelop.Create(EntityName.Contact, runtimeInfo.EntityId, runtimeInfo.CustomerId, runtimeInfo.SubmittedVersion, RuntimeStatus.EVALUATION_COMPLETED, null, runtimeInfo.OrchestrationData), runtimeInfo.CorellationId);
                return;
            }

            var rules = LoadRulesToExecute();
            await rules.RunCheckAsync(runtimeInfo);

            if (rules.Feedbacks.Count == 0)
            {
                // Send EVALUATION_COMPLETED event
                await sender.SendAsync(OrchestrationEnvelop.Create(EntityName.Contact, runtimeInfo.EntityId, runtimeInfo.CustomerId, runtimeInfo.SubmittedVersion, RuntimeStatus.EVALUATION_COMPLETED), runtimeInfo.CorellationId);
            }
            else
            {
                // Send EVALUATION_INCOMPLETE event
                await sender.SendAsync(OrchestrationEnvelop.Create(EntityName.Contact, runtimeInfo.EntityId, runtimeInfo.CustomerId, runtimeInfo.SubmittedVersion, RuntimeStatus.EVALUATION_INCOMPLETE, [.. rules.Feedbacks]), runtimeInfo.CorellationId);
            }
        }

        /// <summary>
        /// This can be a chain of responsibility
        /// </summary>
        /// <returns></returns>
        private Check LoadRulesToExecute()
        {
            var evaluationCompleted = new EvalutionComplete();
            var contactValidationCheck = new ContactValidationCheck(evaluationCompleted);
            return contactValidationCheck;
        }
    }
}

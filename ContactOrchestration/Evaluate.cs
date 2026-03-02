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

        public async Task Run(RuntimeInfo runtimeInfo)
        {
            // Send EVALUATION_STARTED event
            await sender.SendAsync(OrchestrationEnvelop.Create(EntityName.Contact, runtimeInfo.EntityId, runtimeInfo.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED), runtimeInfo.CorellationId);

            // Marker in the WorkflowData that indicate to skip steps and move to next.
            if (runtimeInfo.WorkflowData.Contains("SHORTCUT_COMPLETE_EVALUATION"))
            {
                await sender.SendAsync(OrchestrationEnvelop.Create(EntityName.Contact, runtimeInfo.EntityId, runtimeInfo.SubmittedVersion, RuntimeStatus.EVALUATION_COMPLETED, null, runtimeInfo.WorkflowData), runtimeInfo.CorellationId);
                return;
            }

            var rules = RulesToExecute();
            await rules.RunCheckAsync(runtimeInfo);

            if (rules.Issues.Count == 0)
            {
                // Send EVALUATION_COMPLETED event
                await sender.SendAsync(OrchestrationEnvelop.Create(EntityName.Contact, runtimeInfo.EntityId, runtimeInfo.SubmittedVersion, RuntimeStatus.EVALUATION_COMPLETED), runtimeInfo.CorellationId);
            }
            else
            {
                // Send EVALUATION_INCOMPLETE event
                await sender.SendAsync(OrchestrationEnvelop.Create(EntityName.Contact, runtimeInfo.EntityId, runtimeInfo.SubmittedVersion, RuntimeStatus.EVALUATION_INCOMPLETE, [.. rules.Issues]), runtimeInfo.CorellationId);
            }
        }

        /// <summary>
        /// This can be a chain of responsibility
        /// </summary>
        /// <returns></returns>
        private Check RulesToExecute()
        {
            var evaluationCompleted = new EvalutionComplete(null);
            var contactValidationCheck = new ContactValidationCheck(evaluationCompleted);
            return contactValidationCheck;
        }
    }
}

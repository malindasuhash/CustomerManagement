using ContactOrchestration.Checks;
using StateManagment.Models;

namespace ContactOrchestration
{
    public class Evaluate
    {
        private readonly IEventPublisher eventPublisher;

        public Evaluate(IEventPublisher eventPublisher)
        {
            this.eventPublisher = eventPublisher;
        }

        public async Task Run(RuntimeInfo runtimeInfo)
        {
            // Send EVALUATION_STARTED event
            await eventPublisher.Send(Command(runtimeInfo.EntityId, runtimeInfo.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED));

            var rules = RulesToExecute();
            await rules.RunCheckAsync(runtimeInfo);

            if (rules.Issues.Count == 0)
            {
                // Send EVALUATION_COMPLETED event
                await eventPublisher.Send(Command(runtimeInfo.EntityId, runtimeInfo.SubmittedVersion, RuntimeStatus.EVALUATION_COMPLETED));
            }
            else
            {
                // Send EVALUATION_INCOMPLETE event
                await eventPublisher.Send(Command(runtimeInfo.EntityId, runtimeInfo.SubmittedVersion, RuntimeStatus.EVALUATION_INCOMPLETE, [.. rules.Issues]));
            }
        }

        private Check RulesToExecute()
        {
            var evaluationCompleted = new EvalutionComplete(null);
            var contactValidationCheck = new ContactValidationCheck(evaluationCompleted);
            return contactValidationCheck;
        }

        private OrchestrationEnvelop Command(string entityId, int submittedVersion, RuntimeStatus status, string[] issues = null)
        {
            return new OrchestrationEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                SubmittedVersion = submittedVersion,
                Status = status,
                Messages = issues
            };
        }
    }
}

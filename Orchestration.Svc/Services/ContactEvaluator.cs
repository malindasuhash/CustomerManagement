using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Model;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Services
{
    public class ContactEvaluator : IEvaluator
    {
        private readonly ISender sender;

        public ContactEvaluator(ISender sender)
        {
            this.sender = sender;
        }

        public EntityName Name => EntityName.Contact;

        public async Task Evaluate(RequestData requestData, CancellationToken stoppingToken)
        {
            EvaluationStarted(requestData);

            // Simulate evaluation process
            if (requestData is not ContactRequestData contactRequest) return;

            var feedbacks = new List<Feedback>();

            // Checks to perform:
            if (contactRequest.Submitted.FirstName == null || contactRequest.Submitted.FirstName.StartsWith("test"))
            {
                feedbacks.Add(new Feedback() {Type = FeedbackType.WaitingForExternalRiskChecks, Message = "InvalidFirstName" });
            }

            if (contactRequest.Submitted.PostalAddress == null)
            {
                feedbacks.Add(new Feedback() {Type = FeedbackType.WaitingForExternalRiskChecks, Message = "PostalAddressMissing" });
            }

            if (feedbacks.Count == 0)
            {
                var orchestrationData = new OrchestrationData[]
                {
                    new() { Key = "LastEvaluatedSubmittedVersion", Value = $"{contactRequest.SubmittedVersion}" }
                };

                await sender.SendMessageAsync(OrchestrationEnvelop.Create(EntityName.Contact, requestData.EntityId, requestData.CustomerId, requestData.SubmittedVersion, RuntimeStatus.EVALUATION_COMPLETED, null, orchestrationData), requestData.CorellationId);
            }
            else
            {
                await sender.SendMessageAsync(OrchestrationEnvelop.Create(EntityName.Contact, requestData.EntityId, requestData.CustomerId, requestData.SubmittedVersion, RuntimeStatus.EVALUATION_INCOMPLETE, feedbacks: feedbacks.ToArray()), requestData.CorellationId);
            }
        }

        private void EvaluationStarted(RequestData requestData)
        {
            var envelope = OrchestrationEnvelop.Create(EntityName.Contact, requestData.EntityId, requestData.CustomerId, requestData.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED);
            sender.SendMessageAsync(envelope, requestData.CorellationId);
        }
    }
}
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
        public async Task Evaluate(RequestData requestData, CancellationToken stoppingToken)
        {
            EvaluationStarted(requestData);

            // Simulate evaluation process
            if (requestData is not ContactRequestData contactRequest) return;

            var feedbacks = new List<Feedback>();

            // Checks to perform:
            if (contactRequest.Submitted.FirstName == null || contactRequest.Submitted.FirstName.StartsWith("test"))
            {
                feedbacks.Add(new Feedback() { Type = FeedbackType.Error, Key = "FirstName", Value = "InvalidFirstName" });
            }

            if (contactRequest.Submitted.PostalAddress == null)
            {
                feedbacks.Add(new Feedback() { Type = FeedbackType.Error, Key = "PostalAddress", Value = "PostalAddressMissing" });
            }

            if (feedbacks.Count == 0)
            {
                var orchestrationData = new OrchestrationData[]
                {
                    new() { Key = "LastEvaluatedSubmittedVersion", Value = $"{contactRequest.SubmittedVersion}" }
                };

                await sender.SendAsync(OrchestrationEnvelop.Create(EntityName.Contact, requestData.EntityId, requestData.CustomerId, requestData.SubmittedVersion, RuntimeStatus.EVALUATION_COMPLETED, null, orchestrationData), requestData.CorellationId);
            }
            else
            {
                await sender.SendAsync(OrchestrationEnvelop.Create(EntityName.Contact, requestData.EntityId, requestData.CustomerId, requestData.SubmittedVersion, RuntimeStatus.EVALUATION_INCOMPLETE, feedbacks: feedbacks.ToArray()), requestData.CorellationId);
            }
        }

        private void EvaluationStarted(RequestData requestData)
        {
            var envelope = OrchestrationEnvelop.Create(EntityName.Contact, requestData.EntityId, requestData.CustomerId, requestData.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED);
            sender.SendAsync(envelope, requestData.CorellationId);
        }
    }
}
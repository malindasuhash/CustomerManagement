
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
            var envelope = OrchestrationEnvelop.Create(EntityName.Contact, requestData.EntityId, requestData.CustomerId, requestData.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED);

            await sender.SendAsync(envelope, requestData.CorellationId);
        }
    }
}
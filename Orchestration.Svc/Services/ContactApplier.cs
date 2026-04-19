
using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Model;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Services
{
    public class ContactApplier : IApplier
    {
        private readonly ISender sender;
        private readonly IExternalAdapter externalAdapter;

        public ContactApplier(ISender sender, IExternalAdapter externalAdapter)
        {
            this.sender = sender;
            this.externalAdapter = externalAdapter;
        }

        public EntityName Name => EntityName.Contact;

        public async Task Apply(RequestData requestData, CancellationToken stoppingToken)
        {
            if (requestData is not ContactRequestData contactRequest) { await Task.CompletedTask; return; }

            if (contactRequest.AppliedVersion >= contactRequest.SubmittedVersion)
            {
                // Already applied, probably a touch operation.
                var envelope = OrchestrationEnvelop.Create(EntityName.Contact, requestData.EntityId, requestData.CustomerId, requestData.SubmittedVersion, RuntimeStatus.CHANGE_APPLIED);
                await sender.SendMessageAsync(envelope, requestData.CorellationId);
                return;
            }

            // Are there new changes in the submitted version compared to the last applied version?
            // Note that if a changes has been detected previously, a new case will not be created.
            var changesDetected = externalAdapter.ProcessChanges(contactRequest);

            if (changesDetected.Any())
            {
                ApplyChangesExternally(requestData);
                return;
            }

            // Are there changes that have been detected by other orchestrations but not yet applied to this contact?
            var pendingChangesDetected = externalAdapter.PendingChanges(contactRequest.CustomerId, null, contactRequest.EntityId);

            if (pendingChangesDetected.Any())
            {
                ApplyChangesExternally(requestData);
                return;
            }

            await sender.SendMessageAsync(OrchestrationEnvelop.Create(EntityName.Contact, requestData.EntityId, requestData.CustomerId, requestData.SubmittedVersion, RuntimeStatus.CHANGE_APPLIED), requestData.CorellationId);
        }

        private void ApplyChangesExternally(RequestData requestData)
        {
            var envelope = OrchestrationEnvelop.Create(EntityName.Contact, requestData.EntityId, requestData.CustomerId, requestData.SubmittedVersion, RuntimeStatus.EXTERNAL_CHANGES_DETECTED);
            sender.SendMessageAsync(envelope, requestData.CorellationId);
        }
    }
}
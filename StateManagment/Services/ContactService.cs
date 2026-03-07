using StateManagment.Entity;
using StateManagment.Models;

namespace StateManagment.Services
{
    public class ContactService
    {
        private readonly IChangeProcessor changeProcessor;
        private readonly ICustomerDatabase customerDatabase;

        public ContactService(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase)
        {
            this.changeProcessor = changeProcessor;
            this.customerDatabase = customerDatabase;
        }

        public async Task<TaskOutcome> Delete(string customerId, string entityId, bool submit)
        {
            var envelop = new MessageEnvelop
            {
                EntityId = entityId,
                Change = ChangeType.Delete,
                Name = EntityName.Contact,
                IsSubmitted = submit,
                CustomerId = customerId
            };

            return await changeProcessor.ProcessChangeAsync(envelop);
        }

        public async Task<MessageEnvelop> Get(string customerId, string entityId)
        {
            return await customerDatabase.GetEntityDocument(EntityName.Contact, entityId, customerId);
        }

        public async Task<MessageEnvelop> Patch(Contact contact, string customerId, string entityId, int targetVersion, bool submit)
        {
            var envelop = new MessageEnvelop
            {
                EntityId = entityId,
                Change = ChangeType.Update,
                Name = EntityName.Contact,
                Draft = contact,
                IsSubmitted = submit,
                CustomerId = customerId,
                DraftVersion = targetVersion
            };

            await changeProcessor.ProcessChangeAsync(envelop);

            return await customerDatabase.GetEntityDocument(EntityName.Contact, entityId);
        }

        public async Task<MessageEnvelop> Post(string customerId, Contact contact, bool submit)
        {
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.Contact,
                Draft = contact,
                IsSubmitted = submit,
                CustomerId = customerId
            };
            await changeProcessor.ProcessChangeAsync(envelop);

            return await customerDatabase.GetEntityDocument(EntityName.Contact, envelop.EntityId);
        }

        public async Task<TaskOutcome> Touch(string entityId)
        {
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Touch,
                Name = EntityName.Contact,
                EntityId = entityId
            };

            return await changeProcessor.ProcessChangeAsync(envelop);
        }
    }
}

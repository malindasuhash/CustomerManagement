using StateManagment.Entity;
using StateManagment.Models;

namespace StateManagment.Services
{
    public class CustomerManagementService
    {
        protected readonly IChangeProcessor changeProcessor;
        protected readonly ICustomerDatabase customerDatabase;

        public CustomerManagementService(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase)
        {
            this.changeProcessor = changeProcessor;
            this.customerDatabase = customerDatabase;
        }

        public async Task<MessageEnvelop> Patch<T>(T entity, EntityName entityName, string customerId, string entityId, int targetVersion, bool submit) where T : IEntity, new()
        {
            var envelop = new MessageEnvelop
            {
                EntityId = entityId,
                Change = ChangeType.Update,
                Name = entityName,
                Draft = entity,
                IsSubmitted = submit,
                CustomerId = customerId,
                DraftVersion = targetVersion
            };

            await changeProcessor.ProcessChangeAsync(envelop);

            return await customerDatabase.GetEntityDocument(entityName, entityId);
        }

        public async Task<MessageEnvelop> Post<T>(T entity, EntityName entityName, string customerId,  bool submit) where T : IEntity, new()
        {
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = entityName,
                Draft = entity,
                IsSubmitted = submit,
                CustomerId = customerId
            };
            await changeProcessor.ProcessChangeAsync(envelop);

            return await customerDatabase.GetEntityDocument(entityName, envelop.EntityId);
        }

        public async Task<TaskOutcome> Delete(EntityName entityName, string customerId, string entityId, bool submit)
        {
            var envelop = new MessageEnvelop
            {
                EntityId = entityId,
                Change = ChangeType.Delete,
                Name = entityName,
                IsSubmitted = submit,
                CustomerId = customerId
            };

            return await changeProcessor.ProcessChangeAsync(envelop);
        }

        public async Task<MessageEnvelop> Get(EntityName entityName, string customerId, string entityId)
        {
            return await customerDatabase.GetEntityDocument(entityName, entityId, customerId);
        }

        public async Task<TaskOutcome> Submit(EntityName entityName, string customerId, string entityId, int targetDraftVersion)
        {
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Submit,
                Name = entityName,
                IsSubmitted = true,
                CustomerId = customerId,
                EntityId = entityId,
                DraftVersion = targetDraftVersion
            };

            return await changeProcessor.ProcessChangeAsync(envelop);
        }

        public async Task<TaskOutcome> Touch(MessageEnvelop messageEnvelop)
        {
            return await changeProcessor.ProcessChangeAsync(messageEnvelop);
        }
    }
}

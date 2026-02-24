using StateManagment.Entity;
using StateManagment.Models;

namespace StateManagment.Services
{
    public class ContactService
    {
        private readonly IChangeProcessor changeProcessor;
        private readonly IDataRetriever dataRetriver;

        public ContactService(IChangeProcessor changeProcessor, IDataRetriever dataRetriver)
        {
            this.changeProcessor = changeProcessor;
            this.dataRetriver = dataRetriver;
        }

        public async Task<TaskOutcome> Delete(string entityId, bool submit)
        {
            var envelop = new MessageEnvelop
            {
                EntityId = entityId,
                Change = ChangeType.Delete,
                Name = EntityName.Contact,
                IsSubmitted = submit
            };

            return await changeProcessor.ProcessChangeAsync(envelop);
        }

        public async Task<MessageEnvelop> Get(string entityId)
        {
            return await dataRetriver.GetEntityEnvelop(entityId, EntityName.Contact);
        }

        public async Task<MessageEnvelop> Patch(Contact contact, string entityId, bool submit)
        {
            var envelop = new MessageEnvelop
            {
                EntityId = entityId,
                Change = ChangeType.Update,
                Name = EntityName.Contact,
                Draft = contact,
                IsSubmitted = submit
            };

            await changeProcessor.ProcessChangeAsync(envelop);

            return await dataRetriver.GetEntityEnvelop(envelop.EntityId, envelop.Name);
        }

        public async Task<MessageEnvelop> Post(Contact contact, bool submit)
        {
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.Contact,
                Draft = contact,
                IsSubmitted = submit
            };
            await changeProcessor.ProcessChangeAsync(envelop);

            return await dataRetriver.GetEntityEnvelop(envelop.EntityId, envelop.Name);
        }
    }
}

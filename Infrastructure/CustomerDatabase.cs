using StateManagment.Entity;
using StateManagment.Models;

namespace Infrastructure
{
    public class CustomerDatabase : ICustomerDatabase
    {
        public EntityBasics GetBasicInfo(EntityName entityName, string entityId)
        {
            throw new NotImplementedException();
        }

        public MessageEnvelop GetEntityDocument(EntityName entityName, string entityId)
        {
            throw new NotImplementedException();
        }

        public void MergeDraft(MessageEnvelop envelop, int latestDraftVersion)
        {
            throw new NotImplementedException();
        }

        public void StoreApplied(EntityName entityName, IEntity entity, string entityId)
        {
            throw new NotImplementedException();
        }

        public void StoreDraft(MessageEnvelop messageEnvelop, int incrementalDraftVersion)
        {
            throw new NotImplementedException();
        }

        public void StoreSubmitted(EntityName entityName, IEntity entity, string entityId, string updatedUser)
        {
            throw new NotImplementedException();
        }

        public void UpdateData(EntityName entityName, string entityId, EntityState targetState, string[] messages)
        {
            throw new NotImplementedException();
        }
    }
}

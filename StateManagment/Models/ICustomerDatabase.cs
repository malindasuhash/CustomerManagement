using StateManagment.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public interface ICustomerDatabase
    {
        EntityBasics GetBasicInfo(EntityName entityName, string entityId);
        MessageEnvelop GetEntityDocument(EntityName entityName, string entityId);
        void StoreDraft(MessageEnvelop messageEnvelop, int incrementalDraftVersion);
        void StoreSubmitted(EntityName entityName, IEntity entity, string entityId, int latestDraftVersion);
        void UpdateData(EntityName entityName, string entityId, EntityState targetState, string[] messages);
    }
}

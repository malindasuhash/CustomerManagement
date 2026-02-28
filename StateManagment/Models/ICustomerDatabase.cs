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
        Task<EntityBasics> GetBasicInfo(EntityName entityName, string entityId);
        Task<MessageEnvelop> GetEntityDocument(EntityName entityName, string entityId);
        Task<TaskOutcome> StoreDraft(MessageEnvelop messageEnvelop, int incrementalDraftVersion);
        Task<TaskOutcome> StoreSubmitted(EntityName entityName, IEntity entity, string entityId, string updatedUser);
        Task<TaskOutcome> UpdateData(EntityName entityName, string entityId, EntityState targetState, string[] messages);
        void StoreApplied(EntityName entityName, IEntity entity, string entityId);
        void MergeDraft(MessageEnvelop envelop, int latestDraftVersion);
    }
}

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
        Task<EntityBasics> GetBasicInfo<T>(string entityId) where T : IEntity;
        Task<MessageEnvelop> GetEntity<T>(LookupPredicate predicate) where T : IEntity;
        Task<TaskOutcome> StoreDraft<T>(MessageEnvelop messageEnvelop, int incrementalDraftVersion) where T : IEntity;
        Task<TaskOutcome> StoreSubmitted<T>(IEntity entity, string entityId, string customerId, string updatedUser) where T : IEntity;
        Task<TaskOutcome> UpdateData<T>(string entityId, string customerId, EntityState targetState, Feedback[] feedbacks, OrchestrationData[] orchestrationData, string? legalEntityId = null) where T : IEntity;
        Task<TaskOutcome> StoreApplied<T>(IEntity entity, string entityId, string customerId, bool confirmRemoval) where T : IEntity;
        Task<TaskOutcome> MergeDraft<T>(MessageEnvelop envelop, int latestDraftVersion) where T : IEntity;
        Task<TaskOutcome> MarkForRemoval<T>(string entityId) where T : IEntity;
    }

    public struct LookupPredicate(string entityId, string customerId, string legalEntityId)
    {
        public string EntityId { get; } = entityId;
        public string CustomerId { get; } = customerId;
        public string LegalEntityId { get; } = legalEntityId;

        public static LookupPredicate Create(string entityId, string customerId, string? legalEntityId = null)
        {
            return new LookupPredicate(entityId, customerId, legalEntityId);
        }
    }
}

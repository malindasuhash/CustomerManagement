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
        Task<EntityBasics> GetBasicInfo<T>(LookupPredicate predicate) where T : IEntity;
        Task<MessageEnvelop> GetEntity<T>(LookupPredicate predicate) where T : IEntity;
        Task<TaskOutcome> StoreDraft<T>(MessageEnvelop messageEnvelop, int incrementalDraftVersion) where T : IEntity;
        Task<TaskOutcome> StoreSubmitted<T>(LookupPredicate predicate, string updatedUser) where T : IEntity;
        Task<TaskOutcome> UpdateData<T>(string entityId, string customerId, EntityState targetState, Feedback[] feedbacks, OrchestrationData[] orchestrationData, string? legalEntityId = null) where T : IEntity;
        Task<TaskOutcome> StoreApplied<T>(IEntity entity, string entityId, string customerId, bool confirmRemoval) where T : IEntity;
        Task<TaskOutcome> MergeDraft<T>(MessageEnvelop envelop, int latestDraftVersion) where T : IEntity;
        Task<TaskOutcome> MarkForRemoval<T>(string entityId) where T : IEntity;
    }
}

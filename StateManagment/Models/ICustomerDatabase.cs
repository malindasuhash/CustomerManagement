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
        Task<MessageEnvelop> GetEntity<T>(string entityId, string? customerId = null) where T : IEntity;
        Task<TaskOutcome> StoreDraft<T>(MessageEnvelop messageEnvelop, int incrementalDraftVersion) where T : IEntity;
        Task<TaskOutcome> StoreSubmitted<T>(IEntity entity, string entityId, string updatedUser) where T : IEntity;
        Task<TaskOutcome> UpdateData<T>(string entityId, EntityState targetState, Feedback[] feedbacks, OrchestrationData[] orchestrationData) where T : IEntity;
        Task<TaskOutcome> StoreApplied<T>(IEntity entity, string entityId, bool confirmRemoval) where T : IEntity;
        Task<TaskOutcome> MergeDraft<T>(MessageEnvelop envelop, int latestDraftVersion) where T : IEntity;
        Task<TaskOutcome> MarkForRemoval<T>(string entityId) where T : IEntity;
    }
}

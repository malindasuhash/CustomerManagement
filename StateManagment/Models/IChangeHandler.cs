using StateManagment.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public interface IChangeHandler
    {
        Task<TaskOutcome> ChangeStatusTo<T>(string entityId, string customerId, EntityState entityState, Feedback[]? feedbacks = null, OrchestrationData[]? orchestrationData = null) where T : IEntity;
        Task<TaskOutcome> Draft<T>(MessageEnvelop envelop) where T : IEntity;
        Task<TaskOutcome> ReleaseEntityLock(string entityId);
        Task<TaskOutcome> Submitted<T>(MessageEnvelop envelop) where T : IEntity;
        Task<TaskOutcome> TakeEntityLock(string entityId);
        Task<TaskOutcome> TryMergeDraft<T>(MessageEnvelop envelop) where T : IEntity;
        Task<TaskOutcome> TryLockSubmitted<T>(MessageEnvelop envelop) where T : IEntity;
        Task<TaskOutcome> TryMarkForRemoval<T>(MessageEnvelop envelop) where T : IEntity;
    }
}

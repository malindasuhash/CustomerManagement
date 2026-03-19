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
        Task<TaskOutcome> ChangeStatusTo(string entityId, EntityName name, EntityState entityState, Feedback[]? feedbacks = null, OrchestrationData[]? orchestrationData = null);
        Task<TaskOutcome> Draft<T>(MessageEnvelop envelop) where T : IEntity;
        Task<TaskOutcome> ReleaseEntityLock(string entityId);
        Task<TaskOutcome> Submitted<T>(MessageEnvelop envelop) where T : IEntity;
        Task<TaskOutcome> TakeEntityLock(string entityId);
        Task<TaskOutcome> TryMergeDraft<T>(MessageEnvelop envelop) where T : IEntity;
        Task<TaskOutcome> TryLockSubmitted<T>(MessageEnvelop envelop) where T : IEntity;
        Task<TaskOutcome> TryMarkForRemoval(MessageEnvelop envelop);
    }
}

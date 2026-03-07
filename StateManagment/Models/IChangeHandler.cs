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
        Task<TaskOutcome> Draft(MessageEnvelop envelop);
        Task<TaskOutcome> ReleaseEntityLock(string entityId);
        Task<TaskOutcome> Submitted(MessageEnvelop envelop);
        Task<TaskOutcome> TakeEntityLock(string entityId);
        Task<TaskOutcome> TryMergeDraft(MessageEnvelop envelop);
        Task<TaskOutcome> TryLockSubmitted(MessageEnvelop envelop);
        Task<TaskOutcome> TryMarkForRemoval(MessageEnvelop envelop);
    }
}

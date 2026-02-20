using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public interface IChangeHandler
    {
        Task<TaskOutcome> ChangeStatusTo(string entityId, EntityName name, EntityState entityState, string[]? messages = null);
        void Draft(MessageEnvelop envelop);
        Task<TaskOutcome> ReleaseEntityLock(string entityId);
        void Submitted(MessageEnvelop envelop);
        Task<TaskOutcome> TakeEntityLock(string entityId);
        Task<TaskOutcome> TryDraft(MessageEnvelop envelop);
        Task<TaskOutcome> TryLockSubmitted(MessageEnvelop envelop);
    }
}

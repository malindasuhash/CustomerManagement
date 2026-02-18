using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager.Models
{
    public interface IChangeHandler
    {
        Task<TaskOutcome> ChangeStatusTo(string entityId, EntityName name, EntityState entityState);
        void Draft(MessageEnvelop envelop);
        Task ReleaseEntityLock(string entityId);
        void Submitted(MessageEnvelop envelop);
        Task<TaskOutcome> TakeEntityLock(string entityId);
        bool TryDraft(MessageEnvelop envelop, out TaskOutcome outcome);
        object TryLockSubmitted(MessageEnvelop envelop, out TaskOutcome outcome);
    }
}

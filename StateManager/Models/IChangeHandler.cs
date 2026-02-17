using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager.Models
{
    public interface IChangeHandler
    {
        void Draft(MessageEnvelop envelop);
        Task ReleaseEntityLock(string entityId);
        void Submitted(MessageEnvelop envelop);
        Task TakeEntityLock(string entityId);
        bool TryDraft(MessageEnvelop envelop, out ChangeOutcome outcome);
        object TryLockSubmitted(MessageEnvelop envelop, out ChangeOutcome outcome);
    }
}

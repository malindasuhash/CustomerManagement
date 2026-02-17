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
        void Submitted(MessageEnvelop envelop);
        bool TryDraft(MessageEnvelop envelop, out ProcessOutcome outcome);
        object TrySubmitted(MessageEnvelop envelop, out ProcessOutcome outcome);
    }
}

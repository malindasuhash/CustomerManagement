using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Events
{
    public interface IEventManager
    {
        void Publish(IStateManagerEvent stateManagerEvent);
        void TryDraft(MessageEnvelop envelop, out TaskOutcome outcome);
    }
}

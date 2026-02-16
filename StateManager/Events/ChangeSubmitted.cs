using StateManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager.Events
{
    public class ChangeSubmitted : IStateManagerEvent
    {
        public ChangeSubmitted(MessageEnvelop messageEnvelop)
        {
            MessageEnvelop = messageEnvelop;
        }
        public string Name => throw new NotImplementedException();

        public MessageEnvelop MessageEnvelop { get; }
    }
}

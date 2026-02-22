using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemory
{
    public class SimpleEventPublisher : IEventPublisher
    {
        public Task<TaskOutcome> PublishStateChangedEvent(MessageEnvelop messageEnvelop)
        {
            return Task.FromResult(TaskOutcome.OK);
        }

        public Task<TaskOutcome> Send(OrchestrationEnvelop orchestrationEvent)
        {
            throw new NotImplementedException();
        }
    }
}

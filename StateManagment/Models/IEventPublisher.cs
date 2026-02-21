using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public interface IEventPublisher
    {
        Task<TaskOutcome> PublishStateChangedEvent(MessageEnvelop messageEnvelop);
    }
}

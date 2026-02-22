using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactOrchestration
{
    public class Apply
    {
        private readonly IEventPublisher eventPublisher;

        public Apply(IEventPublisher eventPublisher)
        {
            this.eventPublisher = eventPublisher;
        }

        public async Task Run(RuntimeInfo runtimeInfo)
        {
           // There is nothing to change
           await eventPublisher.Send(new OrchestrationEnvelop
           {
               EntityId = runtimeInfo.EntityId,
               Name = EntityName.Contact,
               SubmittedVersion = runtimeInfo.SubmittedVersion,
               Status = RuntimeStatus.CHANGE_APPLIED
           });
        }
    }
}

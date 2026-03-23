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
        private readonly ISender sender;

        public Apply(ISender sender)
        {
            this.sender = sender;
        }

        public async Task Run(OrchestrationInfo runtimeInfo)
        {
           // There is nothing to change
           await sender.SendAsync(OrchestrationEnvelop.Create(
               EntityName.Contact,
               runtimeInfo.EntityId,
               runtimeInfo.CustomerId,
               runtimeInfo.SubmittedVersion,
               RuntimeStatus.CHANGE_APPLIED
           ), runtimeInfo.CorellationId);
        }
    }
}

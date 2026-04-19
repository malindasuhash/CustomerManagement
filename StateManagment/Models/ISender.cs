using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public interface ISender
    {
        Task<TaskOutcome> SendMessageAsync(OrchestrationEnvelop message, string correlationId);

        Task<TaskOutcome> DataChangedAsync(MessageEnvelop message, string correlationId);
    }
}

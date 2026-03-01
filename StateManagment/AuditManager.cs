using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment
{
    public class AuditManager : IAuditManager
    {
        public Task<TaskOutcome> Write(MessageEnvelop after, MessageEnvelop? before = null)
        {
            return Task.FromResult(TaskOutcome.OK);
        }
    }
}

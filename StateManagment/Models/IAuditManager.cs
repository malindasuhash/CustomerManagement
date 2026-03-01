using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public interface IAuditManager
    {
        Task<TaskOutcome> Write(AuditTarget auditTarget, MessageEnvelop after, MessageEnvelop? before = null);
    }

    public enum AuditTarget
    {
        None = 0,
        Draft,
        Submitted
    }
}

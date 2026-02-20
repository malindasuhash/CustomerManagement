using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public interface IDistributedLock
    {
        Task<TaskOutcome> Lock(string key);
        Task<TaskOutcome> Unlock(string key);
    }
}

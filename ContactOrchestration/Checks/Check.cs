using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactOrchestration.Checks
{
    internal abstract class Check
    {
        protected readonly Check nextCheck;
        public List<string> Issues { get; set; } = [];

        public Check(Check nextCheck)
        {
            this.nextCheck = nextCheck;
        }

        public abstract Task RunCheckAsync(RuntimeInfo runtimeInfo);
    }
}

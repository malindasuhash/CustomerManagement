using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager.Models
{
    public class ProcessOutcome
    {
        public static readonly ProcessOutcome OK = new(true);

        public bool Successful { get; }

        private ProcessOutcome(bool success)
        {
            Successful = success;
        }
    }
}

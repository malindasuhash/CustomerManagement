using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager.Models
{
    public class TaskOutcome
    {
        public static readonly TaskOutcome OK = new(true);
        public static readonly TaskOutcome LOCK_UNAVAILABLE = new(false);
        public static readonly TaskOutcome VERSION_MISMATCH = new(false);
        public static readonly TaskOutcome TRANSITION_NOT_SUPPORTED = new(false);
        public static readonly TaskOutcome CHANGE_NOT_SUPPORTED = new(false);

        public bool Successful { get; }

        private TaskOutcome(bool success)
        {
            Successful = success;
        }
    }
}

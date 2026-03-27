using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public class TaskOutcome
    {
        public static readonly TaskOutcome OK = new(true, "OK");
        public static readonly TaskOutcome LOCK_UNAVAILABLE = new(false, "LOCK_UNAVAILABLE");
        public static readonly TaskOutcome VERSION_MISMATCH = new(false, "VERSION_MISMATCH");
        public static readonly TaskOutcome TRANSITION_NOT_SUPPORTED = new(false, "TRANSITION_NOT_SUPPORTED");
        public static readonly TaskOutcome CHANGE_NOT_SUPPORTED = new(false, "CHANGE_NOT_SUPPORTED");
        public static readonly TaskOutcome STALE_DRAFT = new(false, "STALE_DRAFT");
        public static readonly TaskOutcome NO_CHANGE_TO_SUBMIT = new(false, "NO_CHANGE_TO_SUBMIT");
        public static readonly TaskOutcome NOT_FOUND = new(false, "NOT_FOUND");

        public bool Successful { get; }

        public string Reason { get; }

        private TaskOutcome(bool success, string reason)
        {
            Successful = success;
            Reason = reason;
        }
    }
}

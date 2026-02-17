using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager.Models
{
    public class ChangeOutcome
    {
        public static readonly ChangeOutcome OK = new(true);

        public bool Successful { get; }

        private ChangeOutcome(bool success)
        {
            Successful = success;
        }
    }
}

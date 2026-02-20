using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public class MessageEnvelop
    {
        private EntityState defaultState = EntityState.NEW;

        public ChangeType Change { get; set; }
        public EntityName Name { get; set; }

        public required string EntityId { get; set; }
        public int DraftVersion { get; set; }
        public int SubmittedVersion { get; set; }
        public bool Submitted { get; set; }
        public EntityState State => defaultState;

        public dynamic Draft { get; set; }

        public void SetState(EntityState targetState)
        {
            defaultState = targetState;
        }
    }
}

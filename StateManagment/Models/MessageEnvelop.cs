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

        public string EntityId { get; set; }
        public int DraftVersion { get; set; }
        public int SubmittedVersion { get; set; }
        public int AppliedVersion { get; set; }
        public bool IsSubmitted { get; set; }
        public EntityState State => defaultState;

        public dynamic Draft { get; set; }

        public dynamic Submitted { get; set; }

        public dynamic Applied { get; set; }

        public string UpdateUser { get; set; }
        public DateTime UpdateTimestamp { get; set; }

        public string CreatedUser { get; set; }
        public DateTime CreatedTimestamp { get; set; }

        public void SetState(EntityState targetState)
        {
            defaultState = targetState;
        }

        override public string ToString()
        {
            return $"Change: {Change}, Name: {Name}, EntityId: {EntityId}, DraftVersion: {DraftVersion}, SubmittedVersion: {SubmittedVersion}, IsSubmitted: {IsSubmitted}, State: {State}, CreatedUser: {CreatedUser}, CreatedDate: {CreatedTimestamp}, Draft: <<{Draft}>>, Submitted: <<{Submitted}>>; Applied: <<{Applied}>>";
        }
    }
}

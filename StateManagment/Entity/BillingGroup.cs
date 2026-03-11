using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Entity
{
    public class BillingGroup : IEntity
    {
        public string BillingBankAccountId { get; set; }
        public string Description { get; set; }
        public string LegalEntityId { get; set; }
        public string Name { get; set; }
        public Descriptor[] Descriptors { get; set; } = [];
        public string Label { get; set; }
    }
}

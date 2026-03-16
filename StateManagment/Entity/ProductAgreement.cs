using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Entity
{
    public class ProductAgreement : IEntity, ILegalEntityAttached
    {
        public string DisplayName { get; set; }
        public string ProductType { get; set; }
        public string RateCardId { get; set; }
        public ProductFeature[] Features { get; set; }
        public ProductConfiguration[] Configuration { get; set; }
        public string LegalEntityId { get; set; }
        public Descriptor[] Descriptors { get; set; } = [];
        public string Label { get; set; }
    }
}

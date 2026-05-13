using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Entity
{
    public class BankAccount : IEntity, ILegalEntityAttached
    {
        public string LegalEntityId { get; set; }
        public string[] BankAccountHolderNames { get; set; }
        public string AccountNumber { get; set; }
        public string BankCity { get; set; }
        public string BankCountry { get; set; }
        public string BankName { get; set; }
        public bool BillingDefault { get; set; }
        public string Iban { get; set; }
        public string Name { get; set; }
        public string SortCode { get; set; }
        public string Swift { get; set; }
        public MetaDataModel[] MetaData { get; set; } = [];
        public string[] Labels { get; set; }
        public SystemDataModel[] SystemData { get; set; } = [];
    }
}

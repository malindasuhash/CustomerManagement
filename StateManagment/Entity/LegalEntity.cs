using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Entity
{
    public class LegalEntity : IEntity
    {
        public string Name { get; set; }
        public string TradingName { get; set; }
        public string BusinessType { get; set; }
        public string BusinessEmail { get; set; }
        public BusinessContact[] BusinessContacts { get; set; }
        public int? CardTurnoverPerAnnum { get; set; }
        public int? TurnoverPerAnnum { get; set; }
        public string CompanyRegistration { get; set; }
        public string VatRegistration { get; set; }
        public string VatRegistrationStatus { get; set; }
        public DateTime? DateBusinessStarted { get; set; }
        public DateTime? DateTradingStarted { get; set; }
        public LegalEntityWithControl[] LegalEntitiesWithControl { get; set; }
        public int? MaximumTransactionValue { get; set; }
        public string MerchantCategoryCode { get; set; }
        public string StandardIndustryClassification { get; set; }
        public PersonWithControl[] PersonsWithControl { get; set; }
        public RegisteredAddress[] RegisteredAddresses { get; set; }
        public Descriptor[] Descriptors { get; set; } = [];
        public string Label { get; set; }
    }
}

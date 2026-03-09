using StateManagment.Entity;

namespace Api.ApiModels
{
    public class LegalEntityModel
    {
        public string Name { get; set; }
        public string TradingName { get; set; }
        public string BusinessType { get; set; }
        public string BusinessEmail { get; set; }
        public BusinessContactModel[] BusinessContacts { get; set; }
        public int CardTurnoverPerAnnum { get; set; }
        public int TurnoverPerAnnum { get; set; }
        public string CompanyRegistration { get; set; }
        public string VatRegistration { get; set; }
        public string VatRegistrationStatus { get; set; }
        public string DateBusinessStarted { get; set; }
        public string DateTradingStarted { get; set; }
        public LegalEntityWithControlModel[] LegalEntitiesWithControl { get; set; }
        public int MaximumTransactionValue { get; set; }
        public string MerchantCategoryCode { get; set; }
        public string StandardIndustryClassification { get; set; }
        public PersonWithControlModel[] PersonsWithControl { get; set; }
        public RegisteredAddressModel[] RegisteredAddresses { get; set; }
        public DescriptorModel[] Descriptors { get; set; } = [];
        public string Label { get; set; }
        public int TargetVersion { get; set; } = 0;
    }

}

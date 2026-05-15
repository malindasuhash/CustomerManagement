namespace StateManagment.Entity
{
    public class LegalEntity : IEntity
    {
        public string Name { get; set; }
        public string TradingName { get; set; }
        public BusinessType BusinessType { get; set; }
        public string BusinessEmail { get; set; }
        public BusinessContact[] BusinessContacts { get; set; }
        public string OperatingAs { get; set; }
        public GoodsOwnership GoodsOwnership { get; set; }
        public int? CardTurnoverPerAnnum { get; set; }
        public int? TurnoverPerAnnum { get; set; }
        public string CompanyRegistration { get; set; }
        public string CountryOfAuthority { get; set; }
        public string CharityRegistration { get; set; }
        public string VatRegistration { get; set; }
        public VatRegistrationStatus VatRegistrationStatus { get; set; }
        public DateTime? DateBusinessStarted { get; set; }
        public DateTime? DateTradingStarted { get; set; }
        public LegalEntityWithControl[] LegalEntitiesWithControl { get; set; }
        public int? MaximumTransactionValue { get; set; }
        public string MerchantCategoryCode { get; set; }
        public string StandardIndustryClassification { get; set; }
        public string TradingIndustryClassification { get; set; }
        public string BusinessIdentification { get; set; }
        public PersonWithControl[] PersonsWithControl { get; set; }
        public PartnersWithInterest[] PartnersWithInterest { get; set; }
        public RegisteredAddress[] RegisteredAddresses { get; set; }
        public EndOfBusinessRelationship EndOfBusinessRelationship { get; set; }
        public LegalEntityStatus Status { get; set; }
        public MetaDataModel[] MetaData { get; set; } = [];
        public string[] Labels { get; set; }
        public SystemDataModel[] SystemData { get; set; }
        
    }

    public enum LegalEntityStatus
    {
        Active,
        Inactive,
        Suspended,
        Closed,
        Pending,
        Other
    }

    public enum VatRegistrationStatus
    {
        None,
        Registered,
        NotRegistered,
        Pending,
        Exempt,
        Other
    }

    public enum BusinessType
    {
        SoleProprietorship,
        Partnership,
        Corporation,
        LimitedLiabilityCompany,
        Cooperative,
        NonProfit,
        Other
    }

    public enum GoodsOwnership
    {
        None,
        Owned,
        Leased,
        Rented,
        ThirdParty,
        Other
    }

    public class EndOfBusinessRelationship
    {
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public Dictionary<string, string> AdditionalDetails { get; set; }
    }
}

namespace StateManagment.Entity
{
    public class ProductAgreement : IEntity, ILegalEntityAttached
    {
        public string ProductCatalogueId { get; set; }
        public string ProductId { get; set; }
        public string DisplayName { get; set; }
        public string TradingLocationId { get; set; }
        public string BillingGroupId { get; set; }
        public string ProductType { get; set; }
        public string RateCardId { get; set; }
        public ProductFeature[] Features { get; set; }
        public ProductConfiguration[] Configuration { get; set; }
        public string LegalEntityId { get; set; }
        public MetaDataModel[] MetaData { get; set; } = [];
        public string[] Labels { get; set; }
        public SystemDataModel[] SystemData { get; set; } = [];
    }
}

namespace StateManagment.Entity
{
    public class LegalEntityWithControl : IEntity
    {
        public ControlType[] ControlTypes { get; set; }
        public string LegalEntityId { get; set; }
    }

    public class PartnersWithInterest : IEntity
    {
        public string LegalEntityId { get; set; }
        public Attribution[] Attributions { get; set; }
        public MetaDataModel[] MetaData { get; set; } = [];
    }

    public enum Attribution
    {
        None,
        Introducer,
        Affiliate,
        Reseller,
        Other
    }
}

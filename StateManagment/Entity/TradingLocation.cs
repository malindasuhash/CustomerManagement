namespace StateManagment.Entity
{
    public class TradingLocation : IEntity, ILegalEntityAttached
    {
        public string Name { get; set; }
        public string Website { get; set; }
        public Address Address { get; set; }
        public ContactReference[] Contacts { get; set; }
        public string LegalEntityId { get; set; }

        public string Label { get; set; }
        public MetaData[] Descriptors { get; set; }
    }
}

using StateManagment.Models;

namespace StateManagment.Entity
{
    public class TradingLocation : IEntity, ILegalEntityAttached
    {
        public string Name { get; set; }
        public string Website { get; set; }
        public Address Address { get; set; }
        public ContactReference[] Contacts { get; set; }
        public string LegalEntityId { get; set; }

        public string[] Labels { get; set; }
        public MetaDataModel[] MetaData { get; set; }
        public SystemDataModel[] SystemData { get; set; }
    }
}

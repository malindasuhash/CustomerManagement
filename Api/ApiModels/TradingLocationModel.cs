using StateManagment.Entity;

namespace Api.ApiModels
{
    public class TradingLocationModel
    {
        public string Name { get; set; }
        public string Website { get; set; }
        public AddressModel Address { get; set; }
        public ContactReferenceModel[] Contacts { get; set; }
        public string LegalEntityId { get; set; }

        public string Label { get; set; }
        public DescriptorModel[] Descriptors { get; set; }

        public int TargetVersion { get; set; }
    }
}

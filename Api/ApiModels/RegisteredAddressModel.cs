namespace Api.ApiModels
{
    public class RegisteredAddressModel
    {
        public AddressModel Address { get; set; }
        public bool Current { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
    }
}

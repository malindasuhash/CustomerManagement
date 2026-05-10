using StateManagment.Entity;

namespace Api.ApiModels
{
    // Some considerations:
    // 1. Properties may need to optional so that the serialisation framework
    // can ignore properties without marking them mandatory.
    // 2. Target version is vital as it maps to latest draft version stored.
    public class ContactModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Telephone { get; set; }
        public string? TelephoneCode { get; set; }
        public string? Email { get; set; }
        public string? AltTelephone { get; set; }
        public string? AltTelephoneCode { get; set; }
        public AddressModel? PostalAddress { get; set; }
        public MetaData[]? Descriptors { get; set; }
        public string? Label { get; set; }

        public int TargetVersion { get; set; } = 0;
    }

}

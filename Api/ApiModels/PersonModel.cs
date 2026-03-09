namespace Api.ApiModels
{
    public class PersonModel
    {
        public AddressModel Address { get; set; }
        public string DateOfBirth { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Nationality { get; set; }
    }
}

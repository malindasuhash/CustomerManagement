namespace StateManagment.Entity
{
    public class Person
    {
        public RegisteredAddress[] Addresses { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Nationality { get; set; }
        public string PersonIdentification { get; set; }
    }
}

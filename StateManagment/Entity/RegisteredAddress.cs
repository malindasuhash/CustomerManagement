namespace StateManagment.Entity
{
    public class RegisteredAddress
    {
        public Address Address { get; set; }
        public bool Current { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
    }
}

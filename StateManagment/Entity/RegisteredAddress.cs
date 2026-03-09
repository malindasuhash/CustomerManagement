namespace StateManagment.Entity
{
    public class RegisteredAddress : IEntity
    {
        public Address Address { get; set; }
        public bool Current { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}

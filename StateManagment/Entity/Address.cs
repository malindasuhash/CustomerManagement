namespace StateManagment.Entity
{
    public class Address : IEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Country { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Locality { get; set; }
        public string Region { get; set; }
    }
}

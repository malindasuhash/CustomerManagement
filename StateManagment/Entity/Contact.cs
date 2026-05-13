namespace StateManagment.Entity
{
    public class Contact : IEntity
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public string Telephone { get; set; }
        public string TelephoneCode { get; set; }
        public string Email { get; set; }
        public string AltTelephone { get; set; }
        public string AltTelephoneCode { get; set; }
        public Address PostalAddress { get; set; }
        public MetaDataModel[] MetaData { get; set; } = [];
        public string[] Labels { get; set; }
        public SystemDataModel[] SystemData { get; set; } = [];
        override public string ToString()
        {
            return $"Name: {Name}";
        }
    }
}

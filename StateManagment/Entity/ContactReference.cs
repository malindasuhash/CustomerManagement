namespace StateManagment.Entity
{
    public class ContactReference : IEntity
    {
        public string ContactId { get; set; }
        public ContactType ContactType { get; set; }
    }
}

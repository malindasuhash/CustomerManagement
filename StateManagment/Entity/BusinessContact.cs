namespace StateManagment.Entity
{
    public class BusinessContact : IEntity
    {
        public string ContactId { get; set; }
        public ContactType ContactType { get; set; }
    }
}

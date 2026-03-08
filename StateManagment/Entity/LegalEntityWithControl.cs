namespace StateManagment.Entity
{
    public class LegalEntityWithControl : IEntity
    {
        public string[] ControlTypes { get; set; }
        public string LegalEntityId { get; set; }
    }
}

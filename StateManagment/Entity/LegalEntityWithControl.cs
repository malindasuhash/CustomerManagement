namespace StateManagment.Entity
{
    public class LegalEntityWithControl : IEntity
    {
        public ControlType[] ControlTypes { get; set; }
        public string LegalEntityId { get; set; }
    }
}

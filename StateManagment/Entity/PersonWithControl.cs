namespace StateManagment.Entity
{
    public class PersonWithControl : IEntity
    {
        public ControlType[] ControlTypes { get; set; }
        public Person Person { get; set; }
    }
}

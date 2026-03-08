namespace StateManagment.Entity
{
    public class PersonWithControl : IEntity
    {
        public string[] ControlTypes { get; set; }
        public Person Person { get; set; }
    }
}

namespace StateManagment.Entity
{
    public class PersonWithControl : IEntity
    {
        public ControlType[] ControlTypes { get; set; }
        public Person Person { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Designation { get; set; }
        public int OwnershipPercentage { get; set; }
    }
}

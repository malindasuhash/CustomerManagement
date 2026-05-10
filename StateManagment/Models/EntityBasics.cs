namespace StateManagment.Models
{
    public class EntityBasics
    {
        public EntityName Name { get; set; }
        public string EntityId { get; set; }
        public EntityState State { get; set; }
        public decimal DraftVersion { get; set; }
        public decimal SubmittedVersion { get; set; }
    }
}

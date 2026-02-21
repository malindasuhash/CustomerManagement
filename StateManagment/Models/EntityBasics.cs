namespace StateManagment.Models
{
    public class EntityBasics
    {
        public EntityName Name { get; set; }
        public string EntityId { get; set; }
        public EntityState State { get; set; }
        public int DraftVersion { get; set; }
        public int SubmittedVersion { get; set; }
    }
}

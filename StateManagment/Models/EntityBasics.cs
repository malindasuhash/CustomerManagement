namespace StateManagment.Models
{
    public class EntityBasics
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public EntityState State { get; set; }
        public int DraftVersion { get; set; }
        public int SubmittedVersion { get; set; }
    }
}

namespace StateManager.Models
{
    public class OrchestrationEnvelop
    {
        public required string EntityId { get; set; }
        public EntityName Name { get; set; }
        public int DraftVersion { get; set; }
        public int SubmittedVersion { get; set; }
        public RuntimeStatus Status { get; set; }
        public string[] Messages { get; set; } = [];
    }
}

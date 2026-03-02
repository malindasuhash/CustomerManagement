namespace StateManagment.Models
{
    public class OrchestrationEnvelop
    {
        public required string EntityId { get; set; }
        public EntityName Name { get; set; }
        public int DraftVersion { get; set; }
        public int SubmittedVersion { get; set; }
        public RuntimeStatus Status { get; set; }
        public string[] Messages { get; set; } = [];
        public string[] WorkflowData { get; set; } = [];

        public static OrchestrationEnvelop Create(EntityName entityName, string entityId, int submittedVersion, RuntimeStatus status, string[]? issues = null, string[]? workflowData = null)
        {
            return new OrchestrationEnvelop
            {
                EntityId = entityId,
                Name = entityName,
                SubmittedVersion = submittedVersion,
                Status = status,
                Messages = issues,
                WorkflowData = workflowData
            };
        }
    }
}

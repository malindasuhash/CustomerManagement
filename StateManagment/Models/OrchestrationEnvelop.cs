namespace StateManagment.Models
{
    public class OrchestrationEnvelop
    {
        public required string EntityId { get; set; }
        public EntityName Name { get; set; }
        public int DraftVersion { get; set; }
        public int SubmittedVersion { get; set; }
        public RuntimeStatus Status { get; set; }
        public Feedback[] Feedbacks { get; set; } = [];
        public OrchestrationData[] OrchestrationData { get; set; } = [];

        public static OrchestrationEnvelop Create(EntityName entityName, string entityId, int submittedVersion, RuntimeStatus status, Feedback[]? feedbacks = null, OrchestrationData[]? orchestrationData = null)
        {
            return new OrchestrationEnvelop
            {
                EntityId = entityId,
                Name = entityName,
                SubmittedVersion = submittedVersion,
                Status = status,
                Feedbacks = feedbacks,
                OrchestrationData = orchestrationData
            };
        }
    }
}

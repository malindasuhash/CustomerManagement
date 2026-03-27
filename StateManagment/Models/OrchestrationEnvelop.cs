namespace StateManagment.Models
{
    /// <summary>
    /// This is the message sent back from Orchestration to CM
    /// once a change has been processed.
    /// </summary>
    public class OrchestrationEnvelop
    {
        public required string EntityId { get; set; }
        public required string CustomerId { get; set; }
        public string LegalEntityId { get; set; }
        public EntityName Name { get; set; }
        public int DraftVersion { get; set; }
        public int SubmittedVersion { get; set; }
        public RuntimeStatus Status { get; set; }
        public Feedback[] Feedbacks { get; set; } = [];
        public OrchestrationData[] OrchestrationData { get; set; } = [];

        public static OrchestrationEnvelop Create(EntityName entityName, string entityId, string customerId, int submittedVersion, RuntimeStatus status, Feedback[]? feedbacks = null, OrchestrationData[]? orchestrationData = null, string? legalEntityId = null)
        {
            return new OrchestrationEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
                LegalEntityId = legalEntityId,
                Name = entityName,
                SubmittedVersion = submittedVersion,
                Status = status,
                Feedbacks = feedbacks,
                OrchestrationData = orchestrationData
            };
        }

        public LookupPredicate SearchBy()
        {
            return new LookupPredicate(EntityId, CustomerId, LegalEntityId);
        }
    }
}

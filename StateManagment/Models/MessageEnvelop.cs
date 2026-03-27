using StateManagment.Entity;

namespace StateManagment.Models
{
    public class MessageEnvelop
    {
        public static readonly MessageEnvelop NONE = new() { CustomerId = "NONE" };

        private EntityState defaultState = EntityState.NEW;

        public ChangeType Change { get; set; }
        public EntityName Name { get; set; }

        public required string CustomerId { get; set; }
        public string? EntityId { get; set; }
        public int DraftVersion { get; set; }
        public int SubmittedVersion { get; set; }
        public int AppliedVersion { get; set; }
        public bool IsSubmitted { get; set; }
        public EntityState State
        {
            get { return defaultState; }
            set
            {
                defaultState = value;
            }
        }

        public dynamic? Draft { get; set; }

        public dynamic? Submitted { get; set; }

        public dynamic? Applied { get; set; }

        public string? UpdateUser { get; set; }
        public DateTime? UpdateTimestamp { get; set; }

        public string? CreatedUser { get; set; }
        public DateTime? CreatedTimestamp { get; set; }

        public Feedback[]? Feedback { get; set; }
        public OrchestrationData[]? OrchestrationData { get; set; }

        // Approach for handling deletes

        // This would mean a consumer has requested this entity document to be deleted.
        // Any further updates to this document are prevented unless it is resetted (not implemented yet).
        // Once a "reset" function is provided, then this property may need to be reset
        // back to "false" before any further changes are applied.
        public bool RemoveRequested { get; set; }
        
        // This means the entity document is removed from ciculation unless
        // the client has appropriate permissions to see it (i.e REMOVE_READ).
        // Perhaps if the consumer is a "power user", then such a user
        // should be able to see whether the document is removed.
        // This property will be set during the apply step.
        public bool Removed { get; set; }

        // This property is used to store system level information
        // related to this particular entity. For example, it may include
        // MIDs or ApplicationIDs or third party references that logically
        // belong to this entity. System Data is access controlled
        // e.g. READ_SYSTEM_DATA, SET_SYSTEM_DATA permission.
        public SystemData[]? SystemData { get; set; }

        public void SetState(EntityState targetState)
        {
            defaultState = targetState;
        }

        override public string ToString()
        {
            return $"Change: {Change}, Name: {Name}, EntityId: {EntityId}, DraftVersion: {DraftVersion}, SubmittedVersion: {SubmittedVersion}, IsSubmitted: {IsSubmitted}, State: {State}, CreatedUser: {CreatedUser}, CreatedDate: {CreatedTimestamp}, Draft: <<{Draft}>>, Submitted: <<{Submitted}>>; Applied: <<{Applied}>>";
        }

        public LookupPredicate SearchBy()
        {
            if (Draft is ILegalEntityAttached withLegalEntity)
            {
                return new LookupPredicate(EntityId, CustomerId, withLegalEntity.LegalEntityId);
            }

            return new LookupPredicate(EntityId, CustomerId, null);
        }
    }
}

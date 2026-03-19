namespace StateManagment.Models
{
    public struct LookupPredicate(string entityId, string customerId, string legalEntityId)
    {
        public string EntityId { get; } = entityId;
        public string CustomerId { get; } = customerId;
        public string LegalEntityId { get; } = legalEntityId;

        public static LookupPredicate Create(string entityId, string customerId, string? legalEntityId = null)
        {
            return new LookupPredicate(entityId, customerId, legalEntityId);
        }
    }
}

using StateManagment.Entity;

namespace StateManagment.Models
{
    public interface ICustomerDatabase
    {
        Task<EntityBasics> GetBasicInfo<T>(LookupPredicate predicate) where T : IEntity;
        Task<MessageEnvelop> FindEntity<T>(LookupPredicate predicate) where T : IEntity;
        Task<TaskOutcome> StoreDraft<T>(MessageEnvelop messageEnvelop, decimal incrementalDraftVersion) where T : IEntity;
        Task<TaskOutcome> StoreSubmitted<T>(LookupPredicate predicate, string updatedUser) where T : IEntity;
        Task<TaskOutcome> UpdateData<T>(LookupPredicate predicate, EntityState targetState, Feedback[] feedbacks, OrchestrationData[] orchestrationData) where T : IEntity;
        Task<TaskOutcome> StoreApplied<T>(LookupPredicate predicate, bool confirmRemoval) where T : IEntity;
        Task<TaskOutcome> MergeDraft<T>(MessageEnvelop envelop, decimal latestDraftVersion) where T : IEntity;
        Task<TaskOutcome> MarkForRemoval<T>(LookupPredicate predicate) where T : IEntity;
        Task<List<EntityBasics>> GetPendingChanges(string customerId, string? legalEntityId = null);
        Task<List<MessageEnvelop>> GetLegalEntitiesBy(string customerId, string contactId);
        Task<List<MessageEnvelop>> GetTradingLocationsBy(string customerId, string contactId);
        Task<TaskOutcome> CreateCustomer(CustomerProfile customerProfile);
        Task<CustomerProfile?> GetCustomer(string customerId);
        Task<TaskOutcome> UpdateCustomer(CustomerProfile customerProfile);
    }

    public interface ICustomerProfileRepository
    {
        Task<TaskOutcome> Create(CustomerProfile customerProfile);
        Task<CustomerProfile?> Read(string customerId);
        Task<TaskOutcome> Update(CustomerProfile customerProfile);
        Task<TaskOutcome> Delete(string customerId);
    }
}

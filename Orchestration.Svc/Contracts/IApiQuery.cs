
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Contracts
{
    public interface IApiQuery
    {
        Task<List<MessageEnvelop>> GetLegalEntitiesByContactId(string entityId);
        Task<List<MessageEnvelop>> GetTradingLocationsByContactId(string customerId, string tradingLocationId);
        void TouchLegalEntity(LookupPredicate predicate);
        void TouchTradingLocation(LookupPredicate predicate);
    }

    public class ApiQueryStub : IApiQuery
    {
        public Task<List<MessageEnvelop>> GetLegalEntitiesByContactId(string entityId)
        {
            // Implement logic to retrieve legal entities based on the contact ID
            // For demonstration, returning an empty list
            return Task.FromResult(new List<MessageEnvelop>());
        }
        public Task<List<MessageEnvelop>> GetTradingLocationsByContactId(string customerId, string tradingLocationId)
        {
            // Implement logic to retrieve trading locations based on the customer ID and contact ID
            // For demonstration, returning an empty list
            return Task.FromResult(new List<MessageEnvelop>());
        }
        public void TouchLegalEntity(LookupPredicate predicate)
        {
            // Implement logic to touch legal entity based on the provided predicate
            // For demonstration, this method does nothing
        }
        public void TouchTradingLocation(LookupPredicate predicate)
        {
            // Implement logic to touch trading location based on the provided predicate
            // For demonstration, this method does nothing
        }
    }
}

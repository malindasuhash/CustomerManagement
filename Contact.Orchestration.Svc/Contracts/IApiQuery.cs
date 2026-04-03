
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
}

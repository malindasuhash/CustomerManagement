using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Interfaces
{
    public interface IQueryApi
    {
        IEnumerable<MessageEnvelop> GetLegalEntitiesByContactId(string customerId, string contactId1);
        IEnumerable<MessageEnvelop> GetTradingLocationsByContactId(string customerId, string legalEntityId, string tradingLocationId);
    }
}

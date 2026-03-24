using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Services
{
    public interface IQuery
    {
        IEnumerable<MessageEnvelop> GetLegalEntitiesByContactId(string customerId, string contactId1);
    }
}

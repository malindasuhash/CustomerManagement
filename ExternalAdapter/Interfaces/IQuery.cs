using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Interfaces
{
    public interface IQuery
    {
        IEnumerable<MessageEnvelop> GetLegalEntitiesByContactId(string customerId, string contactId1);
    }
}

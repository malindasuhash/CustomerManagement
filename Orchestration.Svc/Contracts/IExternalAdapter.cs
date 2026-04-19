using Contact.Orchestration.Svc.Model;

namespace Contact.Orchestration.Svc.Contracts
{
    public interface IExternalAdapter
    {
        List<string> ProcessChanges(ContactRequestData contactRequestData);

        List<string> PendingChanges(string customerId, string? legalEntityId, string contactId);
    }

    public class ExternalAdapterStub : IExternalAdapter
    {
        public List<string> ProcessChanges(ContactRequestData contactRequestData)
        {
            // Implement logic to process changes based on the contactRequestData
            // For demonstration, returning an empty list
            return new List<string>();
        }
        public List<string> PendingChanges(string customerId, string? legalEntityId, string contactId)
        {
            // Implement logic to retrieve pending changes based on the provided identifiers
            // For demonstration, returning an empty list
            return new List<string>();
        }
    }
}
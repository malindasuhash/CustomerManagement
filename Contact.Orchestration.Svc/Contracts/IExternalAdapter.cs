using Contact.Orchestration.Svc.Model;

namespace Contact.Orchestration.Svc.Contracts
{
    public interface IExternalAdapter
    {
        List<string> ProcessChanges(ContactRequestData contactRequestData);

        List<string> PendingChanges(string customerId, string? legalEntityId, string contactId);
    }
}

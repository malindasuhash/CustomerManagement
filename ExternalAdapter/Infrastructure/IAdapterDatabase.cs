using ExternalAdapter.Services.AmendContact;

namespace ExternalAdapter.Infrastructure
{
    public interface IAdapterDatabase
    {
        void RegisterChanges(List<ManagementCase> managementCases);
        List<ManagementCase> FindCases(string checksum);
        List<ManagementCase> FindCasesBy(string customerId, string legalEntityId, string entityId);
    }
}

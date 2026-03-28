using ExternalAdapter.Extensions;
using ExternalAdapter.Interfaces;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Services.AmendContact
{
    /// <summary>
    /// Provides a shared assessment routine used by the amend-contact case assessment chain
    /// (e.g. <see cref="MerchantContactCaseAssessment"/>, <see cref="BillingContactUpdateCaseAssessment"/>).
    /// <para>
    /// Given an <see cref="OrchestrationInfo"/> and a target <see cref="ContactType"/>, this class
    /// queries for legal entities associated with the contact, and for each matching business contact
    /// generates an <see cref="CaseType.AmendContact"/> case summary capturing the contact's name change.
    /// </para>
    /// </summary>
    public class AmendContactAssessment
    {
        public static void AssessByAccountType(IQuery query, OrchestrationInfo orchestrationInfo, ContactType expectedContactType, List<CaseSummary> caseSummaries)
        {
           
        }
    }
}

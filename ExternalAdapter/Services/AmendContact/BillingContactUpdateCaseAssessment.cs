using ExternalAdapter.Interfaces;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Services.AmendContact
{
    /// <summary>
    /// This class inspects changes to Billing contact and if a
    /// change is detected, then instructs a maintenance case 
    /// to be created.
    /// </summary>
    public class BillingContactUpdateCaseAssessment : CaseAssessment
    {
        private readonly IQuery query;

        public BillingContactUpdateCaseAssessment(IQuery query, IAsseement nextAssessment) : base(nextAssessment)
        {
            this.query = query;
        }

        public override Task Assess(OrchestrationInfo orchestrationInfo)
        {
            // Query2 /customers/{customer-id}/legal-entities/{legal-entity-id}/trading-locations?contact={contact-id}
            AmendContactAssessment.AssessByAccountType(query, orchestrationInfo, ContactType.Financial, CaseSummaries);

            return next.Assess(orchestrationInfo);
        }
    }
}

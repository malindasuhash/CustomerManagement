using ExternalAdapter.Interfaces;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Services.AmendContact
{
    /// <summary>
    /// Aim of this class is to determine whether AmendContact maintenance
    /// can be created as a result of changes to Merchant account contact.
    /// </summary>
    public class MerchantContactCaseAssessment : CaseAssessment
    {
        private readonly IQuery query;

        public MerchantContactCaseAssessment(IQuery query, CaseAssessment nextAssessment) : base(nextAssessment) 
        {
            this.query = query;
        }

        public override Task Assess(OrchestrationInfo orchestrationInfo)
        {
            // Query1 /customers/{customer-id}/legal-entities?contact={contact-id}
            AmendContactAssessment.AssessByAccountType(query, orchestrationInfo, ContactType.Account, CaseSummaries);
            
            return next.Assess(orchestrationInfo);
        }
    }
}

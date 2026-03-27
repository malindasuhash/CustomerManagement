using ExternalAdapter.Interfaces;
using ExternalAdapter.Services.AmendContact;

namespace ExternalAdapter.Services
{
    public class CaseAssessementBuilder
    {
        private readonly IQuery query;
        private AmendContactChangeAssessor amendContactChangeAssessor;

        public CaseAssessementBuilder(IQuery query)
        {
            this.query = query;
        }

        public void BuildAmendContactAssessment()
        {
            // Amend Contact
            var endOfAssessement = new EndOfAssessement();

            var tradingLocationContactUpdate = new TradingLocationContactUpdateCaseAssessement(query, endOfAssessement);
            var mechantContacUpdate = new MerchantContactCaseAssessment(query, tradingLocationContactUpdate);
            var billingContactUpdate = new BillingContactUpdateCaseAssessment(query, mechantContacUpdate);

            amendContactChangeAssessor = new AmendContactChangeAssessor(billingContactUpdate);
        }

        public AmendContactChangeAssessor GetAmendContactChangeAssessor() { return amendContactChangeAssessor; }
    }
}

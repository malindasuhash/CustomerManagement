using ExternalAdapter.Infrastructure;
using ExternalAdapter.Interfaces;
using ExternalAdapter.Services.AmendContact;

namespace ExternalAdapter.Services
{
    public class CaseAssessementBuilder
    {
        private readonly IQueryApi query;
        private readonly IAdapterDatabase adapterDatabase;
        private AmendContactChangeAssessor amendContactChangeAssessor;

        public CaseAssessementBuilder(IQueryApi query, IAdapterDatabase adapterDatabase)
        {
            this.query = query;
            this.adapterDatabase = adapterDatabase;
        }

        public void Build()
        {
            // Amend Contact
            var endOfAssessement = new EndOfAssessement();

            var tradingLocationContactUpdate = new TradingLocationContactUpdateCaseAssessement(query, endOfAssessement);
            var mechantContacUpdate = new MerchantContactCaseAssessment(query, tradingLocationContactUpdate);
            var billingContactUpdate = new BillingContactUpdateCaseAssessment(query, mechantContacUpdate);

            amendContactChangeAssessor = new AmendContactChangeAssessor(billingContactUpdate, adapterDatabase);
        }

        public AmendContactChangeAssessor Get() { return amendContactChangeAssessor; }
    }
}

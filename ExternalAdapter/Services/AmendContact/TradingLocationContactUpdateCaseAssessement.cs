using ExternalAdapter.Extensions;
using ExternalAdapter.Interfaces;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Services.AmendContact
{
    public class TradingLocationContactUpdateCaseAssessement : CaseAssessment
    {
        private readonly IQuery query;

        public TradingLocationContactUpdateCaseAssessement(IQuery query, CaseAssessment nextAssessement) : base(nextAssessement)
        {
            this.query = query;
        }

        public override Task Assess(OrchestrationInfo runtimeInfo)
        {
            var submittedContact = runtimeInfo.Submitted as Contact;

            var tradingLocations = query.GetTradingLocationsByContactId(runtimeInfo.CustomerId, runtimeInfo.LegalEntityId, runtimeInfo.EntityId);

            foreach (var tradingLocation in tradingLocations)
            {
                var tradingLocationInQuestion = tradingLocation.Submitted as TradingLocation;

                tradingLocationInQuestion?.Contacts
                    .Where(c => c.ContactId.Equals(runtimeInfo.EntityId) && c.ContactType == ContactType.Manager)
                    .ForEach(i => CaseSummaries.Add(new CaseSummary
                    {
                        CaseType = CaseType.AmendContact,
                        CaseNote = "Manager"
                    }));
            }

            return next.Assess(runtimeInfo);
        }
    }
}

using ExternalAdapter.Extensions;
using ExternalAdapter.Interfaces;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Services.AmendContact
{
    /// <summary>
    /// Provides a case assessment that evaluates updates to trading location contacts and creates management cases for
    /// amended contact information.
    /// </summary>
    /// <remarks>This assessment checks for changes to manager contacts associated with trading locations and
    /// generates a pending management case for each affected contact. It is intended to be used as part of a case
    /// assessment chain, where each assessment may process or delegate further as needed.</remarks>
    public class TradingLocationContactUpdateCaseAssessement : CaseAssessment
    {
        private readonly IQueryApi query;

        public TradingLocationContactUpdateCaseAssessement(IQueryApi query, CaseAssessment nextAssessement) : base(nextAssessement)
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
                    .ForEach(i => Case.Add(new ManagementCase()
                    {
                        Origin = runtimeInfo.Origin,
                        CaseType = CaseType.AmendContactTradingLocation,
                        Status = CaseStatus.Candidate,
                        Identifiers = new Dictionary<string, string>
                            {
                                { "CustomerId", runtimeInfo.CustomerId },
                                { "ContactId", runtimeInfo.EntityId }
                            },
                        EntitiesToReevaluate = [EntityName.Contact],
                        Before = runtimeInfo.Applied,
                        After = runtimeInfo.Submitted,
                        Checksum = CryptographyExtensions.GenerateContactChecksum(submittedContact)
                    }));
            }

            return next.Assess(runtimeInfo);
        }

       
    }

    public enum CaseStatus  
    {
        Candidate,
        Pending,
        Open,
        RequireDocuments,
        Closed,
        Rejected,
        Declined
    }
}

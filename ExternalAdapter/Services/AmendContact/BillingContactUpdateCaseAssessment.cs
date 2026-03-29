using ExternalAdapter.Extensions;
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

        public BillingContactUpdateCaseAssessment(IQuery query, CaseAssessment nextAssessment) : base(nextAssessment)
        {
            this.query = query;
        }

        public override Task Assess(OrchestrationInfo runtimeInfo)
        {
            // Query2 /customers/{customer-id}/legal-entities/{legal-entity-id}/trading-locations?contact={contact-id}
            var submittedContact = runtimeInfo.Submitted as Contact;

            var legalEntities = query.GetLegalEntitiesByContactId(runtimeInfo.CustomerId, runtimeInfo.EntityId);
            
            if (legalEntities.Any())
            {
                foreach (var entity in legalEntities)
                {
                    var legalEntityInQuestion = (LegalEntity)entity.Submitted;
                    legalEntityInQuestion?.BusinessContacts
                        .Where(contact => contact.ContactId.Equals(runtimeInfo.EntityId) && contact.ContactType == ContactType.Account)
                        .ForEach(i => Case.Add(new ManagementCase
                        {
                            Origin = runtimeInfo.Origin,
                            CaseType = CaseType.AmendContactBilling,
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
            }

            return next.Assess(runtimeInfo);
        }
    }
}

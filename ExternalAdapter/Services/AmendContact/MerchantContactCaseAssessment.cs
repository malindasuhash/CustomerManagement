using ExternalAdapter.Extensions;
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
        private readonly IQueryApi query;

        public MerchantContactCaseAssessment(IQueryApi query, CaseAssessment nextAssessment) : base(nextAssessment)
        {
            this.query = query;
        }

        public override Task Assess(OrchestrationInfo orchestrationInfo)
        {
            // Query1 /customers/{customer-id}/legal-entities?contact={contact-id}
            var submittedContact = orchestrationInfo.Submitted as Contact;

            var legalEntities = query.GetLegalEntitiesByContactId(orchestrationInfo.CustomerId, orchestrationInfo.EntityId);

            if (legalEntities.Any())
            {
                foreach (var entity in legalEntities)
                {
                    var legalEntityInQuestion = (LegalEntity)entity.Submitted;
                    legalEntityInQuestion?.BusinessContacts
                        .Where(contact => contact.ContactId.Equals(orchestrationInfo.EntityId) && contact.ContactType == ContactType.Account)
                        .ForEach(i => Case.Add(new ManagementCase
                        {
                            Origin = orchestrationInfo.Origin,
                            CaseType = CaseType.AmendContactMerchant,
                            Status = CaseStatus.Candidate,
                            Identifiers = new Dictionary<string, string>
                            {
                                { "CustomerId", orchestrationInfo.CustomerId },
                                { "ContactId", orchestrationInfo.EntityId }
                            },
                            EntitiesToReevaluate = [EntityName.Contact],
                            Before = orchestrationInfo.Applied,
                            After = orchestrationInfo.Submitted,
                            Checksum = CryptographyExtensions.GenerateContactChecksum(submittedContact)
                        }));
                }
            }

            return next.Assess(orchestrationInfo);
        }
    }
}

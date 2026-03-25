using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Services
{
    /// <summary>
    /// Aim of this class is to determine whether AmendContact maintenance
    /// can be created as a result of changes to Merchant account contact.
    /// </summary>
    public class MerchantContactCaseAssessment : CaseAssessment
    {
        private readonly IQuery query;

        public MerchantContactCaseAssessment(IQuery query, IAsseement asseement)
        {
            this.query = query;
            this.next = asseement;
        }

        public override Task Assess(OrchestrationInfo orchestrationInfo)
        {
            var submittedContact = orchestrationInfo.Submitted as Contact;

            // Query1 /customers/{customer-id}/legal-entities?contact={contact-id}
            var legalEntities = query.GetLegalEntitiesByContactId(orchestrationInfo.CustomerId, orchestrationInfo.EntityId);
            if (legalEntities.Any())
            {
                foreach (var entity in legalEntities)
                {
                    var legalEntityInQuestion = (LegalEntity)entity.Submitted;
                    var associatedContact = legalEntityInQuestion.BusinessContacts.FirstOrDefault(contact => contact.ContactId.Equals(orchestrationInfo.EntityId) && contact.ContactType == ContactType.Account);

                    if (associatedContact != null)
                    {
                        CaseSummaries.Add(new CaseSummary
                        {
                            CaseType = CaseType.AmendContact,
                            CaseNote = $"Maintenance change ==> {submittedContact.FirstName} // {submittedContact.LastName}"
                        });
                    }

                }
            }

            // Query2 /customers/{customer-id}/legal-entities/{legal-entity-id}/trading-locations?contact={contact-id}

            // Query trading locations and find difference
            // If differences are found, then its a Admend contact
            return next.Assess(orchestrationInfo);
        }
    }

    public enum CaseType
    {
        NA,
        AmendContact
    }

    public class CaseSummary
    {
        public static CaseSummary NA = new();
        public static CaseSummary ONBOARDING = new();

        public CaseType CaseType { get; set; }
        public dynamic CaseNote { get; set; }
    }
}

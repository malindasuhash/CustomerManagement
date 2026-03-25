using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Services
{
    /// <summary>
    /// Determines whether contact that was changed is linked to Legal Entity business 
    /// contact of type 'Account'.
    /// </summary>
    public class MerchantContactUpdateAssessment
    {
        private readonly IQuery query;
        private readonly IList<CaseSummary> caseSummaries;

        public MerchantContactUpdateAssessment(IQuery query, IList<CaseSummary> caseSummaries)
        {
            this.query = query;
            this.caseSummaries = caseSummaries;
        }

        public void Assess(OrchestrationInfo orchestrationInfo)
        {
            var submittedContact = orchestrationInfo.Submitted as Contact;
            var appliedContact = orchestrationInfo.Applied as Contact;

            // If there are no changes then assessement stops.
            if (submittedContact == appliedContact) return;

            // Merchant contact - Contact type = Account
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
                        caseSummaries.Add(new CaseSummary
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
        }
    }

    public enum CaseType
    {
        NA,
        Onboarding,
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

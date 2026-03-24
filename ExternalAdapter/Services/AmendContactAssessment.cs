using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Services
{
    public class AmendContactAssessment
    {
        private readonly IQuery query;

        public AmendContactAssessment(IQuery query)
        {
            this.query = query;
        }

        public IEnumerable<CaseSummary> GetSummaries(OrchestrationInfo orchestrationInfo)
        {
            var submittedContact = orchestrationInfo.Submitted as Contact;
            var appliedContact = orchestrationInfo.Applied as Contact;

            var caseSummaries = new List<CaseSummary>();

            if (appliedContact == null)
            {
                caseSummaries.Add(CaseSummary.ONBOARDING);
                return caseSummaries; // Handled during onboarding
            }

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

            return caseSummaries;
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

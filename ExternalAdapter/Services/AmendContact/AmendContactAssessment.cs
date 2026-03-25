using ExternalAdapter.Interfaces;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Services.AmendContact
{
    /// <summary>
    /// Helper class to capture common legal entity query capability
    /// needed for amend contact.
    /// </summary>
    public class AmendContactAssessment
    {
        public static void AssessByAccountType(IQuery query, OrchestrationInfo orchestrationInfo, ContactType expectedContactType, List<CaseSummary> caseSummaries)
        {
            var submittedContact = orchestrationInfo.Submitted as Contact;

            var legalEntities = query.GetLegalEntitiesByContactId(orchestrationInfo.CustomerId, orchestrationInfo.EntityId);

            if (legalEntities.Any())
            {
                foreach (var entity in legalEntities)
                {
                    var legalEntityInQuestion = (LegalEntity)entity.Submitted;
                    var associatedContact = legalEntityInQuestion.BusinessContacts.FirstOrDefault(contact => contact.ContactId.Equals(orchestrationInfo.EntityId) && contact.ContactType == expectedContactType);

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
        }
    }
}

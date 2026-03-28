using ExternalAdapter.Interfaces;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Services.AmendContact
{

    public class AmendContactChangeAssessor
    {
        private readonly CaseAssessment asseement;

        public AmendContactChangeAssessor(CaseAssessment asseement)
        {
            this.asseement = asseement;
        }

        public async Task<ManagementCase[]> Run(OrchestrationInfo orchestrationInfo)
        {
            var submittedContact = orchestrationInfo.Submitted as Contact;
            var appliedContact = orchestrationInfo.Applied as Contact;

            // Entity is being created for the first time.
            // Perhaps this is an onboarding case.
            // Assumes that I do not need to do anything.
            if (appliedContact == null) return Array.Empty<ManagementCase>();

            // If there are no changes then assessement stops.
            // Perhaps this is a 'Touch' operation.
            if (submittedContact.Title.Equals(appliedContact.Title)
                && submittedContact.FirstName.Equals(appliedContact.FirstName)
                && submittedContact.LastName.Equals(appliedContact.LastName)
                && submittedContact.Telephone.Equals(appliedContact.Telephone)
                && submittedContact.TelephoneCode.Equals(appliedContact.TelephoneCode)
                && submittedContact.AltTelephone.Equals(appliedContact.AltTelephone)
                && submittedContact.AltTelephoneCode.Equals(appliedContact.AltTelephoneCode)
                && submittedContact.Email.Equals(appliedContact.Email)
                ) return Array.Empty<ManagementCase>();

            await asseement.Assess(orchestrationInfo);

            return [.. asseement.Case];
            // Assessement is complete now, do next.
        }
    }
}

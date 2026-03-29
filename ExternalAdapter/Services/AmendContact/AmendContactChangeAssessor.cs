using ExternalAdapter.Infrastructure;
using ExternalAdapter.Interfaces;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Services.AmendContact
{

    public class AmendContactChangeAssessor
    {
        private readonly CaseAssessment asseement;
        private readonly IAdapterDatabase adapterDatabase;

        public AmendContactChangeAssessor(CaseAssessment asseement, IAdapterDatabase adapterDatabase)
        {
            this.asseement = asseement;
            this.adapterDatabase = adapterDatabase;
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
            if (submittedContact.Title == appliedContact.Title
                  && submittedContact.FirstName == appliedContact.FirstName
                  && submittedContact.LastName == appliedContact.LastName
                  && submittedContact.Telephone == appliedContact.Telephone
                  && submittedContact.TelephoneCode == appliedContact.TelephoneCode
                  && submittedContact.AltTelephone == appliedContact.AltTelephone
                  && submittedContact.AltTelephoneCode == appliedContact.AltTelephoneCode
                  && submittedContact.Email == appliedContact.Email
                  ) return Array.Empty<ManagementCase>();

            await asseement.Assess(orchestrationInfo);

            adapterDatabase.RegisterChanges(asseement.Case);

            return [.. asseement.Case];
            // Assessement is complete now, do next.
        }
    }
}

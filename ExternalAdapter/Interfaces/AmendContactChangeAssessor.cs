using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Interfaces
{

    public class AmendContactChangeAssessor
    {
        private readonly CaseAssessment asseement;

        public AmendContactChangeAssessor(CaseAssessment asseement)
        {
            this.asseement = asseement;
        }

        public async Task Run(OrchestrationInfo orchestrationInfo)
        {
            var submittedContact = orchestrationInfo.Submitted as Contact;
            var appliedContact = orchestrationInfo.Applied as Contact;

            // Entity is being created for the first time.
            // Perhaps this is an onboarding case.
            // Assumes that I do not need to do anything.
            if (appliedContact == null) return;

            // If there are no changes then assessement stops.
            // Perhaps this is a 'Touch' operation.
            // Note: I am doing a very basic First and Last name comparison
            if (submittedContact.FirstName.Equals(appliedContact.FirstName) 
                && submittedContact.LastName.Equals(appliedContact.LastName)) return;

            await asseement.Assess(orchestrationInfo);

            // Assessement is complete now, do next.
        }
    }
}

using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Interfaces
{

    public class ChangeAssessor
    {
        private readonly IAsseement asseement;

        public ChangeAssessor(IAsseement asseement)
        {
            this.asseement = asseement;
        }

        public async Task Run(OrchestrationInfo orchestrationInfo)
        {
            var submittedContact = orchestrationInfo.Submitted as Contact;
            var appliedContact = orchestrationInfo.Applied as Contact;

            // Entity is being created for the first time.
            // Perhaps this is an onboarding case.
            if (appliedContact == null) return;

            // If there are no changes then assessement stops.
            // Perhaps this is a 'Touch' operation.
            if (submittedContact == appliedContact) return;

            await asseement.Assess(orchestrationInfo);
        }
    }
}

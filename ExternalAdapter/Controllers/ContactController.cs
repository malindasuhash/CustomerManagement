using ExternalAdapter.Interfaces;
using ExternalAdapter.Models;
using ExternalAdapter.Services;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Controllers
{
    [ApiController]
    [Route("/contact")]
    public class ContactController : ControllerBase
    {
        private readonly CaseAssessementBuilder caseAssessementBuilder;

        public ContactController(CaseAssessementBuilder caseAssessementBuilder)
        {
            this.caseAssessementBuilder = caseAssessementBuilder;
            caseAssessementBuilder.Build();
        }

        [HttpPost("processChange")]
        public async Task<InspectionResult> ActionableChangeDetection(ContactChange change)
        {
            var assessor = caseAssessementBuilder.Get();

            var runResult = await assessor.Run(ToOrchestrationInfo(change));

            var inspectionResult = InspectionSummary.FromCases(runResult);

            return inspectionResult;
        }

        [HttpPost("/pendingChanges")]
        public async Task<bool> AreTherePendingChanges([FromQuery] string customerId, [FromQuery] string? legalEntityId, [FromQuery] string entityId)
        {
            // .../contact/outstandingChanges?customer=1883&entityId=892784
            // Check pending changes table and return
            return true;
        }

        private OrchestrationInfo ToOrchestrationInfo(ContactChange incoming)
        {
            return new OrchestrationInfo
            {
                Origin = incoming.Origin,
                CorellationId = incoming.CorellationId,
                EntityId = incoming.EntityId,
                CustomerId = incoming.CustomerId,
                LegalEntityId = incoming.LegalEntityId,
                Submitted = incoming.Submitted,
                Applied = incoming.Applied,
                SubmittedVersion = incoming.SubmittedVersion,
                AppliedVersion = incoming.AppliedVersion,
                OrchestrationData = incoming.OrchestrationData,
                SystemData = incoming.SystemData
            };
        }
    }
}

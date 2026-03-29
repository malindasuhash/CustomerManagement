using ExternalAdapter.Infrastructure;
using ExternalAdapter.Models;
using ExternalAdapter.Services;
using ExternalAdapter.Services.AmendContact;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Models;

namespace ExternalAdapter.Controllers
{
    [ApiController]
    [Route("/contact")]
    public class ContactController : ControllerBase
    {
        private readonly CaseAssessementBuilder caseAssessementBuilder;
        private readonly IAdapterDatabase adapterDatabase;

        public ContactController(CaseAssessementBuilder caseAssessementBuilder, IAdapterDatabase adapterDatabase)
        {
            this.caseAssessementBuilder = caseAssessementBuilder;
            this.adapterDatabase = adapterDatabase;
            caseAssessementBuilder.Build();
        }

        [HttpPost("processChange")]
        public async Task<InspectionResult> ActionableChangeDetection(ContactChange change)
        {
            var assessor = caseAssessementBuilder.Get();

            var runResult = await assessor.Run(_(change));

            var inspectionResult = InspectionSummary.FromCases(runResult);

            return inspectionResult;
        }

        [HttpGet("pendingChanges")]
        public async Task<List<ManagementCase>> AreTherePendingChanges([FromQuery] string customerId, [FromQuery] string? legalEntityId, [FromQuery] string contactId)
        {
            var pendingChanges = adapterDatabase.FindCasesBy(customerId, legalEntityId, contactId);

            return pendingChanges;
        }

        private OrchestrationInfo _(ContactChange incoming)
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

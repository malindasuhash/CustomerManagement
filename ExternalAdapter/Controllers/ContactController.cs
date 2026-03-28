using ExternalAdapter.Interfaces;
using ExternalAdapter.Services;
using Microsoft.AspNetCore.Mvc;
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
        }

        [HttpPost("/inspectAndQueue")]
        public async Task<bool> ActionableChangeDetected(OrchestrationInfo orchestrationInfo)
        {
            caseAssessementBuilder.Build();
            
            var assessor = caseAssessementBuilder.Get();

            var result = await assessor.Run(orchestrationInfo);

            return result.Length != 0;
        }

        [HttpPost("/pendingChanges")]
        public async Task<bool> AreTherePendingChanges([FromQuery] string customerId, [FromQuery] string? legalEntityId, [FromQuery] string entityId)
        {
            // .../contact/outstandingChanges?customer=1883&entityId=892784
            // Check pending changes table and return
            return true;
        }
    }
}

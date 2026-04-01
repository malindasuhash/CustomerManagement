using Api.ApiModels;
using Api.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Models;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerDatabase customerDatabase;
        private readonly LinkGenerator linkGenerator;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IHttpContextAccessor httpContextAccessor;

        public CustomerController(ICustomerDatabase customerDatabase, LinkGenerator linkGenerator, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            this.customerDatabase = customerDatabase;
            this.linkGenerator = linkGenerator;
            this.httpClientFactory = httpClientFactory;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("{customerId}/changes")]
        public async Task<ChangeSummary> GetChanges([FromRoute] string customerId, [FromQuery] string? legalEntityId)
        {
            var pendingChanges = await GetLinks(customerId, legalEntityId);

            return new ChangeSummary()
            {
                total = pendingChanges.Length,
                Changes = pendingChanges
            };
        }

        [HttpPost("{customerId}/submit-changes")]
        public async Task<ChangeSummarySubmitResult> ChangeSubmitResults([FromRoute] string customerId, [FromQuery] string? legalEntityId)
        {
            var pendingChanges = await GetLinks(customerId, legalEntityId);
            var changeSubmitter = new ChangeSubmitter(httpClientFactory, httpContextAccessor);

            var submitResults = await changeSubmitter.SubmitAll(pendingChanges);

            return new ChangeSummarySubmitResult()
            {
                total = submitResults.Count,
                Changes = submitResults
            };
        }

        private async Task<ChangeLink[]> GetLinks(string customerId, string? legalEntityId)
        {
            var pendingChanges = await customerDatabase.GetPendingChanges(customerId, legalEntityId);
            return pendingChanges.Select(change => ChangeLink.Create(change, linkGenerator, customerId, legalEntityId)).ToArray();
        }
    }
}

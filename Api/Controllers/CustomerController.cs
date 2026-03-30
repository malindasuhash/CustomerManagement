using Api.ApiModels;
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

        public CustomerController(ICustomerDatabase customerDatabase)
        {
            this.customerDatabase = customerDatabase;
        }

        [HttpGet("{customerId}/changes")]
        public async Task<ChangeSummary> GetChanges([FromRoute] string customerId, [FromQuery] string? legalEntityId)
        {
            var pendingChanges = await customerDatabase.GetPendingChanges(customerId, legalEntityId);

            return new ChangeSummary()
            {
                total = pendingChanges.Count,
                Changes = pendingChanges
            };
        }
    }
}

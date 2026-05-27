using Api.Mappers;
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
        private readonly ILogger<CustomerController> logger;

        public CustomerController(ICustomerDatabase customerDatabase, ILogger<CustomerController> logger)
        {
            this.customerDatabase = customerDatabase;
            this.logger = logger;
        }

        [HttpPost("{customerId}/profile")]
        public async Task<ActionResult<ApiContract.EntityResponse_Customer>> CreateProfile(ApiContract.CreateCustomer customerProfile)
        {
            logger.LogInformation("Creating customer with Id {CustomerId}", customerProfile.Customer_id);

            var profile = ApiContractCreateCustomer_ToModelCustomerProfile.Convert(customerProfile);

            var result = await customerDatabase.CreateCustomer(profile);
            if (result != TaskOutcome.OK)
            {
                logger.LogError("Failed to create customer with Id {CustomerId}. Reason: {Reason}", customerProfile.Customer_id, result.Reason);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiContract.Rfc7807
                {
                    Detail = $"Failed to create customer with Id {customerProfile.Customer_id}. Reason: {result.Reason}",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "#CustomerProfileFailed",
                });
            }

            var storedCustomer = await customerDatabase.GetCustomer(profile.CustomerId);

            var response = CustomerProfile_ToApiContractEntityResponseCustomer.Convert(storedCustomer);

            return response;
        }

        [HttpGet("{customerId}/profile")]
        public async Task<ActionResult<ApiContract.EntityResponse_Customer>> GetProfile([FromRoute] string customerId)
        {
            logger.LogInformation("Getting customer profile for Id {CustomerId}", customerId);

            var storedCustomer = await customerDatabase.GetCustomer(customerId);

            if (storedCustomer == null)
            {
                logger.LogWarning("Customer with Id {CustomerId} not found", customerId);
                return NotFound(new ApiContract.Rfc7807
                {
                    Detail = $"Customer with Id {customerId} not found",
                    Status = StatusCodes.Status404NotFound,
                    Title = "#CustomerProfileNotFound",
                });
            }
            var response = CustomerProfile_ToApiContractEntityResponseCustomer.Convert(storedCustomer);

            return response;
        }
    }
}

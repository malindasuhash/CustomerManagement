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
        private readonly ICustomerProfileRepository customerProfileRepository;
        private readonly ILogger<CustomerController> logger;

        public CustomerController(ICustomerProfileRepository customerDatabase, ILogger<CustomerController> logger)
        {
            this.customerProfileRepository = customerDatabase;
            this.logger = logger;
        }

        [HttpPost()]
        public async Task<ActionResult<ApiContract.EntityResponse_Customer>> CreateProfile(
            [FromBody] ApiContract.CreateCustomer customerProfile)
        {
            logger.LogInformation("Creating customer with Id {CustomerId}", customerProfile.Customer_id);

            var profile = ApiContractCreateCustomer_ToModelCustomerProfile.Convert(customerProfile);

            var result = await customerProfileRepository.Create(profile);
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

            return await GetProfile(profile.CustomerId);
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_Customer>> GetProfile(
            [FromRoute] string customerId)
        {
            logger.LogInformation("Getting customer profile for Id {CustomerId}", customerId);

            var storedCustomer = await customerProfileRepository.Read(customerId);

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

        [HttpPatch("{customerId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_Customer>> UpdateProfile(
            [FromRoute] string customerId,
            [FromBody] ApiContract.UpdateCustomer customerProfile)
        {
            logger.LogInformation("Updating customer with Id {CustomerId}", customerId);

            var profile = ApiContractUpdateCustomer_ToModelCustomerProfile.Convert(customerId, customerProfile);

            var result = await customerProfileRepository.Update(profile);
            if (result == TaskOutcome.NOT_FOUND)
            {
                logger.LogWarning("Customer with Id {CustomerId} not found for update", customerId);
                return NotFound(new ApiContract.Rfc7807
                {
                    Detail = $"Customer with Id {customerId} not found for update",
                    Status = StatusCodes.Status404NotFound,
                    Title = "#CustomerProfileNotFound",
                });
            }
            else if (result != TaskOutcome.OK)
            {
                logger.LogError("Failed to update customer with Id {CustomerId}. Reason: {Reason}", customerId, result.Reason);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiContract.Rfc7807
                {
                    Detail = $"Failed to update customer with Id {customerId}. Reason: {result.Reason}",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "#CustomerProfileUpdateFailed",
                });
            }

            return await GetProfile(customerId);
        }

        [HttpDelete("{customerId}")]
        public async Task DeleteProfile(
            [FromRoute] string customerId)
        {
            throw new NotImplementedException();
        }
    }
}

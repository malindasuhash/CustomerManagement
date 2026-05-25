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

        [HttpPost("{customer}/profile")]
        public async Task<ApiContract.EntityResponse_Customer> CreateProfile(ApiContract.CreateCustomer customerProfile)
        {
            logger.LogInformation("Creating customer with Id {CustomerId}", customerProfile.Customer_id);

            var profile = ApiContractCreateCustomer_ToModelCustomerProfile.Convert(customerProfile);
            
            var result = await customerDatabase.CreateCustomer(profile);

            var storedCustomer = await customerDatabase.GetCustomer(profile.CustomerId);
            //var response = CustomerProfile_ToApiContractEntityResponseCustomer.Convert(result);

            return null;
        }
    }
}

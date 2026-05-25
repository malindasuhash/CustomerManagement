using Api.Controllers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StateManagment.Models;

namespace Api.Tests.Controllers
{
    public class CustomerControllerTests
    {
        private readonly CustomerController customerController;
        private readonly ICustomerDatabase customerDatabase;

        private const string CustomerId = "test-customer-id";
        private const string CustomerName = "Test Customer";

        public CustomerControllerTests()
        {
            customerDatabase = Substitute.For<ICustomerDatabase>();
            customerController = new CustomerController(customerDatabase, Substitute.For<ILogger<CustomerController>>());
        }

        [Fact]
        public void CreateProfile_WhenInvoked_ThenCreateCustomer()
        {
            // Arrange
            customerDatabase.CreateCustomer(Arg.Any<CustomerProfile>()).Returns(TaskOutcome.OK);

            var customerProfile = new ApiContract.CreateCustomer()
            {
                Name = CustomerName,
                Customer_id = CustomerId
            };

            // Act
            var response = customerController.CreateProfile(customerProfile);

            // Assert
            customerDatabase.Received(1).CreateCustomer(Arg.Is<CustomerProfile>(p => p.CustomerId.Equals(CustomerId) && p.Name.Equals(CustomerName)));
        }

        [Fact]
        public async Task CreateProfile_WhenInvoked_ThenCreateCustomer_TheQueryForCustomer()
        {
            // Arrange
            var expectedProfile = new CustomerProfile() { CustomerId = CustomerId, Name = CustomerName };
            customerDatabase.CreateCustomer(Arg.Any<CustomerProfile>()).Returns(TaskOutcome.OK);

            var customerProfile = new ApiContract.CreateCustomer()
            {
                Name = CustomerName,
                Customer_id = CustomerId
            };

            // Act
            await customerController.CreateProfile(customerProfile);

            // Assert
            await customerDatabase.Received(1).GetCustomer(CustomerId);
        }
    }
}

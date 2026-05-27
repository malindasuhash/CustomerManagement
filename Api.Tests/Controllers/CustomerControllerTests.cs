using Api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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

        [Fact]
        public async Task CreateProfile_WhenInvoked_ThenCreateProfileAndReturnsExpectedResult()
        {
            // Arrange
            customerDatabase.CreateCustomer(Arg.Any<CustomerProfile>()).Returns(TaskOutcome.OK);
            customerDatabase.GetCustomer(Arg.Any<string>()).Returns(new CustomerProfile() { CustomerId = CustomerId, Name = CustomerName });

            // Act
            var customerProfile = new ApiContract.CreateCustomer()
            {
                Name = CustomerName,
                Customer_id = CustomerId
            };

            // Act
            var result = await customerController.CreateProfile(customerProfile);

            // Assert
            result.Value.Customer_id.Should().Be(CustomerId);
            result.Value.Should().Be(CustomerName);
        }

        [Fact]
        public async Task CreateProfile_WhenCustomerProfileCannotBeCreated_ThenReturns500Message()
        {
            // Arrange
            customerDatabase.CreateCustomer(Arg.Any<CustomerProfile>()).Returns(TaskOutcome.FAILED);

            // Act
            var customerProfile = new ApiContract.CreateCustomer()
            {
                Name = CustomerName,
                Customer_id = CustomerId
            };

            // Act
            var result = await customerController.CreateProfile(customerProfile);
            var data = ((ObjectResult)result.Result);

            // Assert
            data.StatusCode.Should().Be(500);
            data.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetProfile_WhenCustomerIdIsSupplied_ThenReturnsCustomerProfile()
        {
            // Arrange
            customerDatabase.GetCustomer(Arg.Any<string>()).Returns(new CustomerProfile() { CustomerId = CustomerId, Name = CustomerName });

            // Act
            var storedCustomer = await customerController.GetProfile(CustomerId);

            // Assert
            storedCustomer.Value.Customer_id.Should().Be(CustomerId);
            storedCustomer.Value.Name.Should().Be(CustomerName);
        }

        [Fact]
        public async Task GetProfile_WhenCustomerIdIsSupplied_ThenReturnsNotFound()
        {
            // Arrange
            customerDatabase.GetCustomer(Arg.Any<string>()).Returns((CustomerProfile)null);

            // Act
            var storedCustomer = await customerController.GetProfile(CustomerId);
            var rfc = storedCustomer.Result as NotFoundObjectResult;

            // Assert
            storedCustomer.Result.Should().BeOfType<NotFoundObjectResult>();
            rfc.Value.Should().BeOfType<ApiContract.Rfc7807>();
        }
    }
}

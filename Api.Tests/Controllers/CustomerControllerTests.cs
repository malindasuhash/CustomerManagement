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
        private readonly ICustomerProfileRepository customerProfileRepository;

        private const string CustomerId = "test-customer-id";
        private const string CustomerName = "Test Customer";

        public CustomerControllerTests()
        {
            customerProfileRepository = Substitute.For<ICustomerProfileRepository>();
            customerController = new CustomerController(customerProfileRepository, Substitute.For<ILogger<CustomerController>>());
        }

        [Fact]
        public void CreateProfile_WhenInvoked_ThenCreateCustomer()
        {
            // Arrange
            customerProfileRepository.Create(Arg.Any<CustomerProfile>()).Returns(TaskOutcome.OK);

            var customerProfile = new ApiContract.CreateCustomer()
            {
                Name = CustomerName,
                Customer_id = CustomerId
            };

            // Act
            var response = customerController.CreateProfile(customerProfile);

            // Assert
            customerProfileRepository.Received(1).Create(Arg.Is<CustomerProfile>(p => p.CustomerId.Equals(CustomerId) && p.Name.Equals(CustomerName)));
        }

        [Fact]
        public async Task CreateProfile_WhenInvoked_ThenCreateCustomer_TheQueryForCustomer()
        {
            // Arrange
            var expectedProfile = new CustomerProfile() { CustomerId = CustomerId, Name = CustomerName };
            customerProfileRepository.Create(Arg.Any<CustomerProfile>()).Returns(TaskOutcome.OK);

            var customerProfile = new ApiContract.CreateCustomer()
            {
                Name = CustomerName,
                Customer_id = CustomerId
            };

            // Act
            await customerController.CreateProfile(customerProfile);

            // Assert
            await customerProfileRepository.Received(1).Read(CustomerId);
        }

        [Fact]
        public async Task CreateProfile_WhenInvoked_ThenCreateProfileAndReturnsExpectedResult()
        {
            // Arrange
            customerProfileRepository.Create(Arg.Any<CustomerProfile>()).Returns(TaskOutcome.OK);
            customerProfileRepository.Read(Arg.Any<string>()).Returns(new CustomerProfile() { CustomerId = CustomerId, Name = CustomerName });

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
            customerProfileRepository.Create(Arg.Any<CustomerProfile>()).Returns(TaskOutcome.FAILED);

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
            customerProfileRepository.Read(Arg.Any<string>()).Returns(new CustomerProfile() { CustomerId = CustomerId, Name = CustomerName });

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
            customerProfileRepository.Read(Arg.Any<string>()).Returns((CustomerProfile)null);

            // Act
            var storedCustomer = await customerController.GetProfile(CustomerId);
            var rfc = storedCustomer.Result as NotFoundObjectResult;

            // Assert
            storedCustomer.Result.Should().BeOfType<NotFoundObjectResult>();
            rfc.Value.Should().BeOfType<ApiContract.Rfc7807>();
        }

        [Fact]
        public async Task UpdateProfile_WhenUpdating_ThenTakesLatestChangesAndAppliesThem()
        {
            // Arrange
            customerProfileRepository.Update(Arg.Any<CustomerProfile>()).Returns(TaskOutcome.OK);
            customerProfileRepository.Read(CustomerId).Returns(new CustomerProfile() {  Name = CustomerName, CustomerId = CustomerId });

            // Act
            var result = await customerController.UpdateProfile(CustomerId, new ApiContract.UpdateCustomer()
            {
                Name = CustomerName,
                Meta_data = new ApiContract.MetaData() 
                {
                    { "a", "b" }
                }
            });

            // Assert
            result.Value.Customer_id.Should().Be(CustomerId);
            result.Value.Name.Should().Be(CustomerName);
            await customerProfileRepository.Received(1).Update(Arg.Is<CustomerProfile>(a => a.MetaData.Any(b => b.Key.Equals("a"))));
            await customerProfileRepository.Received(1).Update(Arg.Is<CustomerProfile>(a => a.MetaData.Any(b => b.Value.Equals("b"))));
        }
    }
}

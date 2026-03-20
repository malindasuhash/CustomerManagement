using Api.Controllers;
using FluentAssertions;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Tests
{
    public class BankAccountControllerTests
    {
        [Fact]
        public async Task TouchBankAccount_WhenInvokedThen_ExpectedEnvelopIsCreated()
        {
            // Arrange
            var changeProcessor = Substitute.For<IChangeProcessor>();
            var database = Substitute.For<ICustomerDatabase>();
            var bankAccountController = new BankAccountController(changeProcessor, database);

            // Act
            await bankAccountController.TouchBankAccount("customer1", "legalEntity1", "bankAccount1");

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<BankAccount>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals("bankAccount1")));
        }
    }
}

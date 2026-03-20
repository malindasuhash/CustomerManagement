using Api.ApiModels;
using Api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Tests.Controllers
{
    public class EntityManagementControllerTests
    {
        const string LegalEntityId = "LegalEntityId1";
        const string CustomerId = "CustomerId1";
        const string BankAccountId = "BankAccountId1";

        private readonly MessageEnvelop messageEnvelop;
        private readonly EntityManagementControllerHelper entityManagementControllerHelper;
        private readonly IChangeProcessor changeProcessor;
        private readonly ICustomerDatabase customerDatabase;

        public EntityManagementControllerTests()
        {
            messageEnvelop = new MessageEnvelop()
            {
                CustomerId = CustomerId,
                EntityId = BankAccountId,
                Draft = new BankAccount() { LegalEntityId = LegalEntityId }
            };

            changeProcessor = Substitute.For<IChangeProcessor>();
            customerDatabase = Substitute.For<ICustomerDatabase>();
            customerDatabase.FindEntity<BankAccount>(Arg.Any<LookupPredicate>()).Returns(messageEnvelop);

            entityManagementControllerHelper = new EntityManagementControllerHelper(changeProcessor, customerDatabase);
        }

        [Fact]
        public async Task GetById_WhenInvoked_ThenQueryDatabaseForEntity()
        {
            // Act
            await entityManagementControllerHelper.CallGetById<BankAccount>(messageEnvelop.SearchBy());

            // Assert
            await customerDatabase.Received(1).FindEntity<BankAccount>(Arg.Is<LookupPredicate>(p => p.CustomerId.Equals(CustomerId) && p.EntityId.Equals(BankAccountId) && p.LegalEntityId.Equals(LegalEntityId)));
        }

        [Fact]
        public async Task GetById_WhenEntityIsNotFound_ThenReturnNotFoundMessage()
        {
            // Arrange
            customerDatabase.FindEntity<BankAccount>(Arg.Any<LookupPredicate>()).Returns(MessageEnvelop.NONE);

            // Act
            var result = await entityManagementControllerHelper.CallGetById<BankAccount>(messageEnvelop.SearchBy());
            var notFound = result.Result as NotFoundObjectResult;

            // Assert
            notFound.Should().NotBeNull();
            notFound.Value.Should().Be(TaskOutcome.NOT_FOUND);
        }
    }

    internal class EntityManagementControllerHelper : EntityManagementController
    {
        public EntityManagementControllerHelper(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase)
            : base(changeProcessor, customerDatabase)
        {
        }

        public Task<ActionResult<EntityDocumentModel>> CallGetById<T>(LookupPredicate predicate) where T : IEntity
        {
            return GetById<T>(predicate);
        }
    }
}

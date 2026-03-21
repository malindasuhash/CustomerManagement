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
            await customerDatabase.Received(1).FindEntity<BankAccount>(Arg.Is<LookupPredicate>(p => PredicateMatch(p)));
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

        [Fact]
        public async Task Process_OnceEntityIsCreated_ThenReturnsIt()
        {
            // Arrange
            changeProcessor.ProcessChangeAsync<BankAccount>(Arg.Any<MessageEnvelop>()).Returns(TaskOutcome.OK);

            // Act
            var result = await entityManagementControllerHelper.CallProcess<BankAccount>(messageEnvelop);

            // Assert
            await customerDatabase.Received(1).FindEntity<BankAccount>(Arg.Is<LookupPredicate>(p => PredicateMatch(p)));
        }

        [Fact]
        public async Task Proess_WhenProcessIsNotSuccessful_ThenReturnsBadRequest()
        {
            // Arrange
            changeProcessor.ProcessChangeAsync<BankAccount>(Arg.Any<MessageEnvelop>()).Returns(TaskOutcome.LOCK_UNAVAILABLE);

            // Act
            var result = await entityManagementControllerHelper.CallProcess<BankAccount>(messageEnvelop);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest.Value.Should().Be(TaskOutcome.LOCK_UNAVAILABLE);
        }

        private static bool PredicateMatch(LookupPredicate lookupPredicate)
        {
            return lookupPredicate.EntityId.Equals(BankAccountId) && lookupPredicate.CustomerId.Equals(CustomerId) && lookupPredicate.LegalEntityId.Equals(LegalEntityId);
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

        public Task<ActionResult<EntityDocumentModel>> CallProcess<T>(MessageEnvelop envelop) where T : IEntity
        {
            return Process<T>(envelop);
        }
    }
}

using Api.Controllers;
using FluentAssertions;
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
        public async Task SubmitForProcessing_OnceEntityIsCreated_ThenReturnsIt()
        {
            // Arrange
            changeProcessor.ProcessChangeAsync<BankAccount>(Arg.Any<MessageEnvelop>()).Returns(TaskOutcome.OK);

            // Act
            var result = await entityManagementControllerHelper.CallSubmitForProcessing<BankAccount>(messageEnvelop);

            // Assert
            await customerDatabase.Received(1).FindEntity<BankAccount>(Arg.Is<LookupPredicate>(p => PredicateMatch(p)));
        }

        [Fact]
        public async Task SubmitForProcessing_WhenProcessIsNotSuccessful_ThenReturnsNotFound()
        {
            // Arrange
            changeProcessor.ProcessChangeAsync<BankAccount>(Arg.Any<MessageEnvelop>()).Returns(TaskOutcome.LOCK_UNAVAILABLE);

            // Act
            var result = await entityManagementControllerHelper.CallSubmitForProcessing<BankAccount>(messageEnvelop);

            // Assert
           result.Should().Be(MessageEnvelop.NONE);
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

        public async Task<MessageEnvelop> CallSubmitForProcessing<T>(MessageEnvelop messageEnvelop) where T : IEntity
        {
            return await base.SubmitForProcessing<T>(messageEnvelop);
        }
    }
}

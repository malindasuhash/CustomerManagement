using FluentAssertions;
using NSubstitute;
using StateManagment;
using StateManagment.Models;

namespace StateManagment.Tests
{
    public class ChangeProcessorTests
    {
        [Fact]
        public async Task ProcessChangeAsync_WhenChangeIsNew_ThenAddsItAsDraft()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 1,
                SubmittedVersion = 0,
                IsSubmitted = false
            };
            var changeHandler = Substitute.For<IChangeHandler>();

            var changeProcessor = new ChangeProcessor(changeHandler, Substitute.For<IStateManager>());

            // Act
            await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            changeHandler.Received(1).Draft(envelop);
        }

        [Fact]
        public async Task ProcessChangeAsync_WhenChangeIsSubmitted_ThenAddsItAsDraftAndSubmitted()
        {
            // Arrange
            var respository = Substitute.For<IRepository>();
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 1,
                SubmittedVersion = 5,
                IsSubmitted = true
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(envelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));

            var stateManager = Substitute.For<IStateManager>();

            var change = new ChangeProcessor(changeHandler, stateManager);

            // Act
            await change.ProcessChangeAsync(envelop);

            // Assert
            changeHandler.Received(1).Draft(envelop);
            changeHandler.Received(1).Submitted(Arg.Any<MessageEnvelop>());
            await stateManager.Received(1).ProcessUpdateAsync(Arg.Any<OrchestrationEnvelop>());
        }


        [Fact]
        public async Task ProcessChangeAsync_WhenUpdatedButNotSubmitted_ThenAddsItAsDraft()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Update,
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 2,
                SubmittedVersion = 1,
                IsSubmitted = false
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            var stateManager = Substitute.For<IStateManager>();

            changeHandler.TryDraft(envelop).Returns(TaskOutcome.OK);

            var changeProcessor = new ChangeProcessor(changeHandler, Substitute.For<IStateManager>());

            // Act
            var result = await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await changeHandler.Received(1).TryDraft(Arg.Any<MessageEnvelop>());
        }

        [Fact]
        public async Task ProcessChangeAsync_WhenUpdatedAndSubmitted_ThenInitiatesNewOrchestration()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Update,
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 2,
                SubmittedVersion = 1,
                IsSubmitted = true
            };
            var changeHandler = Substitute.For<IChangeHandler>();

            changeHandler.TakeEntityLock(envelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));

            var stateManager = Substitute.For<IStateManager>();

            var changeProcessor = new ChangeProcessor(changeHandler, stateManager);

            changeHandler.TryDraft(envelop).Returns(TaskOutcome.OK);
            changeHandler.TryLockSubmitted(envelop).Returns(TaskOutcome.OK);


            // Act
            await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await changeHandler.Received(1).TryDraft(Arg.Any<MessageEnvelop>());
            await changeHandler.Received(1).TryLockSubmitted(Arg.Any<MessageEnvelop>());
            await stateManager.Received(1).ProcessUpdateAsync(Arg.Any<OrchestrationEnvelop>());
        }

        [Fact]
        public async Task ProcessChangeAsync_WhenUpdatedAndSubmittedButDraftFails_ThenDoesNotSubmit()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Update,
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 2,
                SubmittedVersion = 1,
                IsSubmitted = true
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            var changeProcessor = new ChangeProcessor(changeHandler, Substitute.For<IStateManager>());
            changeHandler.TryDraft(envelop).Returns(TaskOutcome.VERSION_MISMATCH);

            // Act
            await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await changeHandler.Received(1).TryDraft(Arg.Any<MessageEnvelop>());
            await changeHandler.DidNotReceive().TryLockSubmitted(Arg.Any<MessageEnvelop>());
        }

        [Fact]
        public async Task ProcessChangeAsync_WhenUpdatedAndSubmittedButLockFails_ThenDoesNotSubmit()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Update,
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 2,
                SubmittedVersion = 1,
                IsSubmitted = true
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TryDraft(envelop).Returns(TaskOutcome.OK);
            changeHandler.TryLockSubmitted(envelop).Returns(TaskOutcome.LOCK_UNAVAILABLE);

            var stateManager = Substitute.For<IStateManager>();
            var changeProcessor = new ChangeProcessor(changeHandler, stateManager);

            // Act
            var result = await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await changeHandler.Received(1).TryDraft(Arg.Any<MessageEnvelop>());
            await changeHandler.Received(1).TryLockSubmitted(Arg.Any<MessageEnvelop>());
            result.Successful.Should().BeFalse();
            await stateManager.DidNotReceive().ProcessUpdateAsync(Arg.Any<OrchestrationEnvelop>());
        }

        [Fact]
        public async Task ProcessChangeAsync_WhenChangeIsNotSupported_ThenReturnsUnsupported()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.None,
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 2,
                SubmittedVersion = 1,
                IsSubmitted = true
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TryDraft(envelop).Returns(TaskOutcome.OK);
            changeHandler.TryLockSubmitted(envelop).Returns(TaskOutcome.LOCK_UNAVAILABLE);

            var stateManager = Substitute.For<IStateManager>();
            var changeProcessor = new ChangeProcessor(changeHandler, stateManager);

            // Act
            var result = await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            result.Successful.Should().BeFalse();
            result.Should().Be(TaskOutcome.CHANGE_NOT_SUPPORTED);
        }
    }
}

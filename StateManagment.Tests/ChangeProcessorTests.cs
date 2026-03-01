using FluentAssertions;
using NSubstitute;
using StateManagment;
using StateManagment.Models;

namespace StateManagment.Tests
{
    public class ChangeProcessorTests
    {
        [Fact]
        public async Task ProcessChangeAsync_WhenJustSubmittedSpecifically_ThenSubmitsRequest()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Submit,
                Name = EntityName.Contact,
                EntityId = "123",
                IsSubmitted = false
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            var changeProcessor = new ChangeProcessor(changeHandler, Substitute.For<IStateManager>());

            // Act
            await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await changeHandler.Received(1).TryLockSubmitted(envelop);
        }

        [Fact]
        public async Task ProcessChangeAsync_WhenSubmitCannotBeCompletes_ThenReturnUnavialable()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Submit,
                Name = EntityName.Contact,
                EntityId = "123",
                IsSubmitted = false
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TryLockSubmitted(envelop).Returns(TaskOutcome.LOCK_UNAVAILABLE);
            var changeProcessor = new ChangeProcessor(changeHandler, Substitute.For<IStateManager>());

            // Act
            var result = await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            result.Should().Be(TaskOutcome.LOCK_UNAVAILABLE);
        }

        [Fact]
        public async Task ProcessChangeAsync_WhenSubmitCompletes_ThenInitiatesStateManager()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Submit,
                Name = EntityName.Contact,
                EntityId = "123",
                IsSubmitted = false
            };

            var stateManager = Substitute.For<IStateManager>();
            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TryLockSubmitted(envelop).Returns(TaskOutcome.OK);
            var changeProcessor = new ChangeProcessor(changeHandler, stateManager);

            // Act
            var result = await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await stateManager.Received(1).Initiate(EntityName.Contact, envelop.EntityId);
        }

        [Fact]
        public async Task ProcessChangeAsync_WhenDeletedButNotSubmitted_ThenUpdatesDraft()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Delete,
                Name = EntityName.Contact,
                EntityId = "123",
                IsSubmitted = false
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            var changeProcessor = new ChangeProcessor(changeHandler, Substitute.For<IStateManager>());

            // Act
            await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await changeHandler.Received(1).TryMergeDraft(envelop);
        }

        [Fact]
        public async Task ProcessChangeAsync_WhenDeletedAndSubmitted_ThenReturnsUnsupported()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Delete,
                Name = EntityName.Contact,
                EntityId = "123",
                IsSubmitted = true
            };
            var stateManager = Substitute.For<IStateManager>();
            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TryLockSubmitted(Arg.Any<MessageEnvelop>()).Returns(TaskOutcome.OK);
            var changeProcessor = new ChangeProcessor(changeHandler, stateManager);

            // Act
            var result = await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await changeHandler.Received(1).TryMergeDraft(envelop);
            await changeHandler.Received(1).TryLockSubmitted(envelop);
            await stateManager.Received(1).Initiate(envelop.Name, envelop.EntityId);
        }

        [Fact]
        public async Task ProcessChangeAsync_WhenDeletedAndSubmittedButLockFails_ThenReturnsUnsupported()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Delete,
                Name = EntityName.Contact,
                EntityId = "123",
                IsSubmitted = true
            };
            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TryLockSubmitted(envelop).Returns(TaskOutcome.LOCK_UNAVAILABLE);
            var stateManager = Substitute.For<IStateManager>();
            var changeProcessor = new ChangeProcessor(changeHandler, stateManager);

            // Act
            var result = await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await changeHandler.Received(1).TryMergeDraft(envelop);
            await changeHandler.Received(1).TryLockSubmitted(envelop);
            result.Successful.Should().BeFalse();
            await stateManager.DidNotReceive().Initiate(Arg.Any<EntityName>(), Arg.Any<string>());
        }

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
            await changeHandler.Received(1).Draft(envelop);
            changeHandler.Received(1).Submitted(Arg.Any<MessageEnvelop>());
            await stateManager.Received(1).Initiate(envelop.Name, envelop.EntityId);
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

            changeHandler.TryMergeDraft(envelop).Returns(TaskOutcome.OK);

            var changeProcessor = new ChangeProcessor(changeHandler, Substitute.For<IStateManager>());

            // Act
            var result = await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await changeHandler.Received(1).TryMergeDraft(Arg.Any<MessageEnvelop>());
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

            changeHandler.TryMergeDraft(envelop).Returns(TaskOutcome.OK);
            changeHandler.TryLockSubmitted(envelop).Returns(TaskOutcome.OK);

            // Act
            await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await changeHandler.Received(1).TryMergeDraft(Arg.Any<MessageEnvelop>());
            await changeHandler.Received(1).TryLockSubmitted(Arg.Any<MessageEnvelop>());
            await stateManager.Received(1).Initiate(envelop.Name, envelop.EntityId);
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
            changeHandler.TryMergeDraft(envelop).Returns(TaskOutcome.VERSION_MISMATCH);

            // Act
            await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await changeHandler.Received(1).TryMergeDraft(Arg.Any<MessageEnvelop>());
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
            changeHandler.TryMergeDraft(envelop).Returns(TaskOutcome.OK);
            changeHandler.TryLockSubmitted(envelop).Returns(TaskOutcome.LOCK_UNAVAILABLE);

            var stateManager = Substitute.For<IStateManager>();
            var changeProcessor = new ChangeProcessor(changeHandler, stateManager);

            // Act
            var result = await changeProcessor.ProcessChangeAsync(envelop);

            // Assert
            await changeHandler.Received(1).TryMergeDraft(Arg.Any<MessageEnvelop>());
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
            changeHandler.TryMergeDraft(envelop).Returns(TaskOutcome.OK);
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

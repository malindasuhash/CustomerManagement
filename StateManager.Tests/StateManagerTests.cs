using NSubstitute;
using StateManager.Events;
using StateManager.Models;

namespace StateManager.Tests
{
    public class StateManagerTests
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
                Submitted = false
            };
            var changeHandler = Substitute.For<IChangeHandler>();

            var stateManager = new StateManager(changeHandler);

            // Act
            await stateManager.ProcessChangeAsync(envelop);

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
                SubmittedVersion = 0,
                Submitted = true
            };

            var changeHandler = Substitute.For<IChangeHandler>();

            var stateManager = new StateManager(changeHandler);

            // Act
            await stateManager.ProcessChangeAsync(envelop);

            // Assert
            changeHandler.Received(1).Draft(envelop);
            changeHandler.Received(1).Submitted(Arg.Is<MessageEnvelop>(x => x.DraftVersion == 1 && x.SubmittedVersion == 1));
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
                Submitted = false
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            
            changeHandler.TryDraft(envelop, out _).Returns(true);

            var stateManager = new StateManager(changeHandler);

            // Act
            var result = await stateManager.ProcessChangeAsync(envelop);

            // Assert
            changeHandler.Received(1).TryDraft(Arg.Any<MessageEnvelop>(), out Arg.Is<ProcessOutcome>(p => p == null));
        }

        [Fact]
        public async Task ProcessChangeAsync_WhenUpdatedAndSubmitted_ThenAddsItAsDraftAndSubmitedOnlyIfDraftSucceeds()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Update,
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 2,
                SubmittedVersion = 1,
                Submitted = true
            };
            var changeHandler = Substitute.For<IChangeHandler>();
            var stateManager = new StateManager(changeHandler);
            

            changeHandler.TryDraft(envelop, out _).Returns(true);
            changeHandler.TrySubmitted(envelop, out _).Returns(true);


            // Act
            await stateManager.ProcessChangeAsync(envelop);

            // Assert
            changeHandler.Received(1).TryDraft(Arg.Any<MessageEnvelop>(), out Arg.Is<ProcessOutcome>(p => p == null));
            changeHandler.Received(1).TrySubmitted(Arg.Any<MessageEnvelop>(), out Arg.Is<ProcessOutcome>(p => p == null));
        }
    }
}

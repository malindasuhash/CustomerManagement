using NSubstitute;
using StateManager.Events;
using StateManager.Models;

namespace StateManager.Tests
{
    public class StateManagerTests
    {
        [Fact]
        public void StateManager_WhenChangeIsProcessed_ThenEvents()
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
            var eventManager = Substitute.For<IEventManager>();

            var stateManager = new StateManager(eventManager);

            // Act
            stateManager.ProcessChange(envelop);

            // Assert
            eventManager.Received(1).Publish(Arg.Any<ChangeDetected>());
        }

        [Fact]
        public void StateManager_WhenChangeIsSubmitted_ThenEvents()
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

            var eventManager = Substitute.For<IEventManager>();
            var stateManager = new StateManager(eventManager);

            // Act
            stateManager.ProcessChange(envelop);

            // Assert
            eventManager.Received(1).Publish(Arg.Any<ChangeDetected>());
            eventManager.Received(1).Publish(Arg.Is<ChangeSubmitted>(x => x.MessageEnvelop.DraftVersion == 1 && x.MessageEnvelop.SubmittedVersion == 1));
        }
    }
}

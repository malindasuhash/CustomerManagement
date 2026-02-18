using FluentAssertions;
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

            var stateManager = new StateManager(changeHandler, Substitute.For<IOrchestrator>());

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
                SubmittedVersion = 5,
                Submitted = true
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(envelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));

            var orchestrator = Substitute.For<IOrchestrator>();

            var dataRetriever = Substitute.For<IDataRetriever>();
            dataRetriever.GetEntityBasics(envelop.EntityId, envelop.Name)
                .Returns(new EntityBasics { State = EntityState.NEW, SubmittedVersion = 5 });

            var stateManager = new StateManager(changeHandler, orchestrator, dataRetriever);

            // Act
            await stateManager.ProcessChangeAsync(envelop);

            // Assert
            changeHandler.Received(1).Draft(envelop);
            changeHandler.Received(1).Submitted(Arg.Any<MessageEnvelop>());
            await orchestrator.Received(1).EvaluateAsync(Arg.Any<MessageEnvelop>());
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

            var stateManager = new StateManager(changeHandler, Substitute.For<IOrchestrator>());

            // Act
            var result = await stateManager.ProcessChangeAsync(envelop);

            // Assert
            changeHandler.Received(1).TryDraft(Arg.Any<MessageEnvelop>(), out Arg.Is<TaskOutcome>(p => p == null));
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
            var orchestrator = Substitute.For<IOrchestrator>();
            var stateManager = new StateManager(changeHandler, orchestrator);


            changeHandler.TryDraft(envelop, out _).Returns(true);
            changeHandler.TryLockSubmitted(envelop, out _).Returns(true);


            // Act
            await stateManager.ProcessChangeAsync(envelop);

            // Assert
            changeHandler.Received(1).TryDraft(Arg.Any<MessageEnvelop>(), out Arg.Is<TaskOutcome>(p => p == null));
            changeHandler.Received(1).TryLockSubmitted(Arg.Any<MessageEnvelop>(), out Arg.Is<TaskOutcome>(p => p == null));
            await orchestrator.Received(1).EvaluateAsync(envelop);
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
                Submitted = true
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            var stateManager = new StateManager(changeHandler, Substitute.For<IOrchestrator>());
            changeHandler.TryDraft(envelop, out _).Returns(false);

            // Act
            await stateManager.ProcessChangeAsync(envelop);

            // Assert
            changeHandler.Received(1).TryDraft(Arg.Any<MessageEnvelop>(), out Arg.Is<TaskOutcome>(p => p == null));
            changeHandler.DidNotReceive().TryLockSubmitted(Arg.Any<MessageEnvelop>(), out Arg.Any<TaskOutcome>());
        }

        [Fact]
        public async Task ProcessUpdateAsync_WhenEntityLockCannotBeTaken_ThenReturnsCannotTakeLockResponse()
        {
            var orchestrationEnvelop = new OrchestrationEnvelop
            {
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 2,
                SubmittedVersion = 1,
                Status = RuntimeStatus.EVALUATION_STARTED
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.LOCK_UNAVAILABLE));

            var orchestrator = Substitute.For<IOrchestrator>();

            var stateManager = new StateManager(changeHandler, orchestrator);

            // Act
            var result = await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).TakeEntityLock(orchestrationEnvelop.EntityId);
            result.Should().Be(TaskOutcome.LOCK_UNAVAILABLE);
        }

        [Fact]
        public async Task ProcessUpdateAsync_OnceLockIsTaken_ThenReleasesIt()
        {
            var orchestrationEnvelop = new OrchestrationEnvelop
            {
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 2,
                SubmittedVersion = 1,
                Status = RuntimeStatus.EVALUATION_STARTED
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));

            var dataRetriever = Substitute.For<IDataRetriever>();
            dataRetriever.GetEntityBasics(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(new EntityBasics { State = EntityState.None, DraftVersion = 5 });

            var stateManager = new StateManager(changeHandler, Substitute.For<IOrchestrator>(), dataRetriever);

            // Act
            var result = await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).TakeEntityLock(orchestrationEnvelop.EntityId);
            await changeHandler.Received(1).ReleaseEntityLock(orchestrationEnvelop.EntityId);
        }

        [Fact]
        public async Task ProcessUpdateAsync_WhenEntityIsNEW_ChangeStatusToEvaluating()
        {
            var orchestrationEnvelop = new OrchestrationEnvelop
            {
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 10,
                SubmittedVersion = 5,
                Status = RuntimeStatus.EVALUATION_STARTED
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));

            var dataRetriever = Substitute.For<IDataRetriever>();
            dataRetriever.GetEntityBasics(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(new EntityBasics { State = EntityState.NEW, SubmittedVersion = 5 });

            var stateManager = new StateManager(changeHandler, Substitute.For<IOrchestrator>(), dataRetriever);

            // Act
            await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.EVALUATING);
        }

        [Fact]
        public async Task ProcessUpdateAsync_WhenSubmittedVersionIsHigher_ThenSetStatusToReevaluationAndStartsEvaluation() 
        {
            // Arrange
            var orchestrationEnvelop = new OrchestrationEnvelop
            {
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 10,
                SubmittedVersion = 5,
                Status = RuntimeStatus.EVALUATION_STARTED
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));

            var dataRetriever = Substitute.For<IDataRetriever>();
            dataRetriever.GetEntityBasics(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(new EntityBasics { State = EntityState.NEW, SubmittedVersion = 6 });

            MessageEnvelop returnThis = new() { EntityId = orchestrationEnvelop.EntityId, Name = orchestrationEnvelop.Name };
            dataRetriever.GetEntityEnvelop(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(returnThis);

            var orchestrator = Substitute.For<IOrchestrator>();
            var stateManager = new StateManager(changeHandler, orchestrator, dataRetriever);

            // Act
            await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.EVALUATION_RESTARTING);

            await orchestrator.Received(1).EvaluateAsync(Arg.Is<MessageEnvelop>(me => returnThis.EntityId.Equals(me.EntityId)));
        }

        [Fact]
        public async Task ProcessUpdateAsync_WhenTransitionIsNotSupported_ThenReturnsTransitionNotSupportedResponse()
        {
            // Arrange
            var orchestrationEnvelop = new OrchestrationEnvelop
            {
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 10,
                SubmittedVersion = 5,
                Status = RuntimeStatus.EVALUATION_STARTED
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));

            var dataRetriever = Substitute.For<IDataRetriever>();
            dataRetriever.GetEntityBasics(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(new EntityBasics { State = EntityState.None, SubmittedVersion = 6 });

            MessageEnvelop returnThis = new() { EntityId = orchestrationEnvelop.EntityId, Name = orchestrationEnvelop.Name };
            dataRetriever.GetEntityEnvelop(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(returnThis);

            var orchestrator = Substitute.For<IOrchestrator>();
            var stateManager = new StateManager(changeHandler, orchestrator, dataRetriever);

            // Act
            var result = await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            result.Should().Be(TaskOutcome.TRANSITION_NOT_SUPPORTED);
        }
    }
}

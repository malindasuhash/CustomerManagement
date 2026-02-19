using FluentAssertions;
using NSubstitute;
using StateManager.Events;
using StateManager.Models;

namespace StateManager.Tests
{
    public class StateManagerTests
    {
      

       

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

            var stateManager = new StateManager(changeHandler, orchestrator, Substitute.For<IDataRetriever>());

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

        [Theory]
        [InlineData(EntityState.NEW)]
        [InlineData(EntityState.IN_REVIEW)]
        public async Task ProcessUpdateAsync_WhenEntityIsNEW_ChangeStatusToEvaluating(EntityState currentState)
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
                .Returns(new EntityBasics { State = currentState, SubmittedVersion = 5 });

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

        [Fact]
        public async Task ProcessUpdateAsync_WhenEvaluationCompletesSuccessfully_ThenChangesStatusToInProgress()
        {
            // Arrange
            var orchestrationEnvelop = new OrchestrationEnvelop
            {
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 10,
                SubmittedVersion = 5,
                Status = RuntimeStatus.EVALUATION_COMPLETED
            };

            // TODO: Consider messages being part of evaluation result
            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));

            var dataRetriever = Substitute.For<IDataRetriever>();
            dataRetriever.GetEntityBasics(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(new EntityBasics { State = EntityState.EVALUATING, SubmittedVersion = 5 });

            MessageEnvelop returnThis = new() { EntityId = orchestrationEnvelop.EntityId, Name = orchestrationEnvelop.Name };
            dataRetriever.GetEntityEnvelop(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(returnThis);

            var orchestrator = Substitute.For<IOrchestrator>();
            var stateManager = new StateManager(changeHandler, orchestrator, dataRetriever);

            // Act
            await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.IN_PROGRESS);
            await orchestrator.Received(1).ApplyAsync(Arg.Any<MessageEnvelop>());
        }

        [Fact]
        public async Task ProcessUpdateAsync_WhenEvaluationIsInComplete_ThenUpdatesMessagesAndState()
        {
            // Arrange
            var orchestrationEnvelop = new OrchestrationEnvelop
            {
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 10,
                SubmittedVersion = 5,
                Status = RuntimeStatus.EVALUATION_INCOMPLETE,
                Messages = ["AwaitingDocumentSigning"]
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));

            var dataRetriever = Substitute.For<IDataRetriever>();
            dataRetriever.GetEntityBasics(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(new EntityBasics { State = EntityState.EVALUATING, SubmittedVersion = 5 });

            MessageEnvelop returnThis = new() { EntityId = orchestrationEnvelop.EntityId, Name = orchestrationEnvelop.Name };
            dataRetriever.GetEntityEnvelop(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(returnThis);

            var orchestrator = Substitute.For<IOrchestrator>();
            var stateManager = new StateManager(changeHandler, orchestrator, dataRetriever);

            // Act
            await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.ATTENTION_REQUIRED, orchestrationEnvelop.Messages);
            await orchestrator.DidNotReceive().ApplyAsync(Arg.Any<MessageEnvelop>());
        }

        [Fact]
        public async Task ProcessUpdateAsync_WhenEvaluationRequireReview_ThenUpdatesStatusToInReview()
        {
            // Arrange
            var orchestrationEnvelop = new OrchestrationEnvelop
            {
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 10,
                SubmittedVersion = 5,
                Status = RuntimeStatus.EVALUATION_REQUIRES_REVIEW,
                Messages = ["NeedsRiskCheckApproval"]
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));
            var dataRetriever = Substitute.For<IDataRetriever>();

            dataRetriever.GetEntityBasics(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(new EntityBasics { State = EntityState.EVALUATING, SubmittedVersion = 5 });

            MessageEnvelop returnThis = new() { EntityId = orchestrationEnvelop.EntityId, Name = orchestrationEnvelop.Name };
            dataRetriever.GetEntityEnvelop(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(returnThis);

            var orchestrator = Substitute.For<IOrchestrator>();
            var stateManager = new StateManager(changeHandler, orchestrator, dataRetriever);

            // Act
            await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.IN_REVIEW, orchestrationEnvelop.Messages);
            await orchestrator.DidNotReceive().ApplyAsync(Arg.Any<MessageEnvelop>());
        }

        [Fact]
        public async Task ProcessUpdateAsync_WhenEntityIsInProgress_ThenDoesNotAllowEvaluationToStart()
        {
            var orchestrationEnvelop = new OrchestrationEnvelop
            {
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 10,
                SubmittedVersion = 6,
                Status = RuntimeStatus.EVALUATION_STARTED
            };
            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));
            var dataRetriever = Substitute.For<IDataRetriever>();
            dataRetriever.GetEntityBasics(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name)
                .Returns(new EntityBasics { State = EntityState.IN_PROGRESS, SubmittedVersion = 5 });
            var stateManager = new StateManager(changeHandler, Substitute.For<IOrchestrator>(), dataRetriever);
           
            // Act
            var result = await stateManager.ProcessUpdateAsync(orchestrationEnvelop);
            
            // Assert
            result.Should().Be(TaskOutcome.TRANSITION_NOT_SUPPORTED);
        }
    }
}

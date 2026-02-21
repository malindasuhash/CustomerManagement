using FluentAssertions;
using NSubstitute;
using StateManagment.Models;
using System;

namespace StateManagment.Tests
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

            var stateManager = new StateManager(changeHandler, orchestrator, Substitute.For<ICustomerDatabase>());

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

            var dataRetriever = Substitute.For<ICustomerDatabase>();
            dataRetriever.GetBasicInfo(orchestrationEnvelop.Name, orchestrationEnvelop.EntityId)
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
                Status = RuntimeStatus.EVALUATION_STARTED,
                Messages = ["EvaluationCompletedSuccessfully"]
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));

            var database = Substitute.For<ICustomerDatabase>();
            database.GetBasicInfo(orchestrationEnvelop.Name, orchestrationEnvelop.EntityId)
                .Returns(new EntityBasics { State = currentState, SubmittedVersion = 5 });

            var stateManager = new StateManager(changeHandler, Substitute.For<IOrchestrator>(), database);

            // Act
            await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.EVALUATING, orchestrationEnvelop.Messages);
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

            var database = Substitute.For<ICustomerDatabase>();
            database.GetBasicInfo(orchestrationEnvelop.Name, orchestrationEnvelop.EntityId)
                .Returns(new EntityBasics { State = EntityState.NEW, SubmittedVersion = 6 });

            var orchestrator = Substitute.For<IOrchestrator>();
            var stateManager = new StateManager(changeHandler, orchestrator, database);

            // Act
            await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.EVALUATION_RESTARTING);

            await orchestrator.Received(1).EvaluateAsync(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
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

            var database = Substitute.For<ICustomerDatabase>();
            database.GetBasicInfo(orchestrationEnvelop.Name, orchestrationEnvelop.EntityId)
                .Returns(new EntityBasics { State = EntityState.None, SubmittedVersion = 5 });

            var orchestrator = Substitute.For<IOrchestrator>();
            var stateManager = new StateManager(changeHandler, orchestrator, database);

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
                Status = RuntimeStatus.EVALUATION_COMPLETED,
                Messages = ["EvaluationCompletedSuccessfully"]
            };

            // TODO: Consider messages being part of evaluation result
            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));

            var database = Substitute.For<ICustomerDatabase>();
            database.GetBasicInfo(orchestrationEnvelop.Name, orchestrationEnvelop.EntityId)
                .Returns(new EntityBasics { State = EntityState.EVALUATING, SubmittedVersion = 5 });

            var orchestrator = Substitute.For<IOrchestrator>();
            var stateManager = new StateManager(changeHandler, orchestrator, database);

            // Act
            await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.IN_PROGRESS, orchestrationEnvelop.Messages);
            await orchestrator.Received(1).ApplyAsync(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
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

            var database = Substitute.For<ICustomerDatabase>();
            database.GetBasicInfo(orchestrationEnvelop.Name, orchestrationEnvelop.EntityId)
                .Returns(new EntityBasics { State = EntityState.EVALUATING, SubmittedVersion = 5 });

            var orchestrator = Substitute.For<IOrchestrator>();
            var stateManager = new StateManager(changeHandler, orchestrator, database);

            // Act
            await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.ATTENTION_REQUIRED, orchestrationEnvelop.Messages);
            await orchestrator.DidNotReceive().ApplyAsync(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
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
                Status = RuntimeStatus.EVALUATION_REQUIRES_MANUAL_REVIEW,
                Messages = ["NeedManagerCheckApproval"]
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));
            var database = Substitute.For<ICustomerDatabase>();

            database.GetBasicInfo(orchestrationEnvelop.Name, orchestrationEnvelop.EntityId)
                .Returns(new EntityBasics { State = EntityState.EVALUATING, SubmittedVersion = 5 });

            var orchestrator = Substitute.For<IOrchestrator>();
            var stateManager = new StateManager(changeHandler, orchestrator, database);

            // Act
            await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.IN_REVIEW, orchestrationEnvelop.Messages);
            await orchestrator.DidNotReceive().ApplyAsync(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
        }

        [Fact]
        public async Task ProcessUpdateAsync_WhenEntityIsInProgress_ThenDoesNotAllowEvaluationToStart()
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
            var database = Substitute.For<ICustomerDatabase>();
            database.GetBasicInfo(orchestrationEnvelop.Name, orchestrationEnvelop.EntityId)
                .Returns(new EntityBasics { State = EntityState.IN_PROGRESS, SubmittedVersion = 5 });
            var stateManager = new StateManager(changeHandler, Substitute.For<IOrchestrator>(), database);

            // Act
            var result = await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            result.Should().Be(TaskOutcome.TRANSITION_NOT_SUPPORTED);
        }

        [Fact]
        public async Task ProcessUpdateAsync_WhenInReviewAndRequiresAdditionalInput_ThenChangeStatusToAttentionRequired()
        {
            // Arrange
            var orchestrationEnvelop = new OrchestrationEnvelop
            {
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 10,
                SubmittedVersion = 6,
                Status = RuntimeStatus.EVALUATION_INCOMPLETE,
                Messages = ["NeedAdditionalInformation"]
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));
            var database = Substitute.For<ICustomerDatabase>();

            database.GetBasicInfo(orchestrationEnvelop.Name, orchestrationEnvelop.EntityId)
                .Returns(new EntityBasics { State = EntityState.IN_REVIEW, SubmittedVersion = 6 });
            var stateManager = new StateManager(changeHandler, Substitute.For<IOrchestrator>(), database);

            // Act
            await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.ATTENTION_REQUIRED, orchestrationEnvelop.Messages);
        }

        [Fact]
        public async Task ProcessUpdateAsync_WhenInProgressAndItFails_ThenChangeStatusToAttentionRequired() 
        {
            // Arrange
            var orchestrationEnvelop = new OrchestrationEnvelop
            {
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 10,
                SubmittedVersion = 6,
                Status = RuntimeStatus.CHANGE_FAILED,
                Messages = ["ServiceUnavailable"]
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));
            var database = Substitute.For<ICustomerDatabase>();

            database.GetBasicInfo(orchestrationEnvelop.Name, orchestrationEnvelop.EntityId  )
                .Returns(new EntityBasics { State = EntityState.IN_PROGRESS, SubmittedVersion = 6 });
            var stateManager = new StateManager(changeHandler, Substitute.For<IOrchestrator>(), database);

            // Act
            await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(1).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, EntityState.ATTENTION_REQUIRED, orchestrationEnvelop.Messages);
        }

        private const int StateDoesNotChange = 0;
        private const int StateChanges = 1;
        private const int EvaluationStarts = 1;
        private const int EvaluationDoesNotStart = 0;
        private const int ApplyStarts = 1;
        private const int ApplyDoesNotStart = 0;
        private const int PostApplyDoesNotStart = 0;
        private const int PostApplyStarts = 1;


        [Theory]
        [InlineData(EntityState.NEW, EntityState.NEW, RuntimeStatus.INITIATE, StateDoesNotChange, EvaluationStarts, ApplyDoesNotStart)]
        [InlineData(EntityState.NEW, EntityState.EVALUATING, RuntimeStatus.EVALUATION_STARTED, StateChanges, EvaluationDoesNotStart, ApplyDoesNotStart)]
        [InlineData(EntityState.EVALUATING, EntityState.ATTENTION_REQUIRED, RuntimeStatus.EVALUATION_INCOMPLETE, StateChanges, EvaluationDoesNotStart, ApplyDoesNotStart)]
        [InlineData(EntityState.EVALUATING, EntityState.IN_REVIEW, RuntimeStatus.EVALUATION_REQUIRES_MANUAL_REVIEW, StateChanges, EvaluationDoesNotStart, ApplyDoesNotStart)]
        [InlineData(EntityState.EVALUATING, EntityState.IN_PROGRESS, RuntimeStatus.EVALUATION_COMPLETED, StateChanges, EvaluationDoesNotStart, ApplyStarts)]
        [InlineData(EntityState.IN_REVIEW, EntityState.IN_REVIEW, RuntimeStatus.INITIATE, StateDoesNotChange, EvaluationStarts, ApplyDoesNotStart)]
        [InlineData(EntityState.IN_REVIEW, EntityState.EVALUATING, RuntimeStatus.EVALUATION_STARTED, StateChanges, EvaluationDoesNotStart, ApplyDoesNotStart)]
        [InlineData(EntityState.ATTENTION_REQUIRED, EntityState.ATTENTION_REQUIRED, RuntimeStatus.INITIATE, StateDoesNotChange, EvaluationStarts, ApplyDoesNotStart)]
        [InlineData(EntityState.ATTENTION_REQUIRED, EntityState.EVALUATING, RuntimeStatus.EVALUATION_STARTED, StateChanges, EvaluationDoesNotStart, ApplyDoesNotStart)]
        [InlineData(EntityState.IN_PROGRESS, EntityState.ATTENTION_REQUIRED, RuntimeStatus.CHANGE_FAILED, StateChanges, EvaluationDoesNotStart, ApplyDoesNotStart)]
        [InlineData(EntityState.IN_PROGRESS, EntityState.SYNCHRONISED, RuntimeStatus.CHANGE_APPLIED, StateChanges, EvaluationDoesNotStart, ApplyDoesNotStart, PostApplyStarts)]
        [InlineData(EntityState.SYNCHRONISED, EntityState.SYNCHRONISED, RuntimeStatus.INITIATE, StateDoesNotChange, EvaluationStarts, ApplyDoesNotStart)]
        [InlineData(EntityState.SYNCHRONISED, EntityState.EVALUATING, RuntimeStatus.EVALUATION_STARTED, StateChanges, EvaluationDoesNotStart, ApplyDoesNotStart)]
        [InlineData(EntityState.EVALUATION_RESTARTING, EntityState.EVALUATING, RuntimeStatus.EVALUATION_STARTED, StateChanges, EvaluationDoesNotStart, ApplyDoesNotStart)]
        public async Task ProcessUpdateAsync_VerifyState_Transitions(EntityState currentState, EntityState targetState, RuntimeStatus action, int statusChangeCount, int evalutionCount, int applyCount, int postApplyCount = 0)
        {
            // Arrange
            var orchestrationEnvelop = new OrchestrationEnvelop
            {
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 10,
                SubmittedVersion = 6,
                Status = action,
                Messages = ["Messages"]
            };

            var changeHandler = Substitute.For<IChangeHandler>();
            changeHandler.TakeEntityLock(orchestrationEnvelop.EntityId).Returns(Task.FromResult(TaskOutcome.OK));
            var database = Substitute.For<ICustomerDatabase>();

            var orchestrator = Substitute.For<IOrchestrator>();

            database.GetBasicInfo(orchestrationEnvelop.Name, orchestrationEnvelop.EntityId)
                .Returns(new EntityBasics { State = currentState, SubmittedVersion = 6 });
            var stateManager = new StateManager(changeHandler, orchestrator, database);

            // Act
            var result = await stateManager.ProcessUpdateAsync(orchestrationEnvelop);

            // Assert
            await changeHandler.Received(statusChangeCount).ChangeStatusTo(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name, targetState, orchestrationEnvelop.Messages);
            await orchestrator.Received(evalutionCount).EvaluateAsync(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
            await orchestrator.Received(applyCount).ApplyAsync(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
            await orchestrator.Received(postApplyCount).PostApplyAsync(orchestrationEnvelop.EntityId, orchestrationEnvelop.Name);
            result.Should().Be(TaskOutcome.OK);
        }
    }
}

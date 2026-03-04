using FluentAssertions;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Tests
{
    public class ChangeHandlerTests
    {
        [Fact]
        public async Task Draft_WhenEntityIsDrafted_StoresEntityUnderDraftAndAudits_AndPublishDataChangedEvent()
        {
            // Arrange
            var database = Substitute.For<ICustomerDatabase>();
            var auditManager = Substitute.For<IAuditManager>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            var changeHandler = new ChangeHandler(database, Substitute.For<IDistributedLock>(), eventPublisher, auditManager);
            var messageEnvelop = new MessageEnvelop
            {
                EntityId = "entity1",
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" }
            };

            database.GetEntityDocument(EntityName.Contact, messageEnvelop.EntityId).Returns(messageEnvelop);

            // Act
            await changeHandler.Draft(messageEnvelop);

            // Assert
            await database.Received(1).StoreDraft(messageEnvelop, messageEnvelop.DraftVersion + 1);
            await database.Received(1).GetEntityDocument(EntityName.Contact, messageEnvelop.EntityId);
            await auditManager.Received(1).Write(AuditTarget.Draft, messageEnvelop);
            await eventPublisher.Received(1).DataChangedAsync(messageEnvelop);
        }

        [Fact]
        public async Task Submitted_WhenEntityIsSubmitted_ThenStoresAuditsAndInDatastore_AndPublishDataChangedEvent()
        {
            // Arrange
            var database = Substitute.For<ICustomerDatabase>();
            var auditManager = Substitute.For<IAuditManager>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            var changeHandler = new ChangeHandler(database, Substitute.For<IDistributedLock>(), eventPublisher, auditManager);
            var messageEnvelop = new MessageEnvelop
            {
                EntityId = "entity1",
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 3,
                UpdateUser = "testUser"
            };

            var messageEnvelop2 = new MessageEnvelop
            {
                EntityId = "entity1",
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Pink", LastName = "Blue" },
                DraftVersion = 3,
                UpdateUser = "testUser"
            };

            database.GetEntityDocument(EntityName.Contact, messageEnvelop.EntityId).Returns(messageEnvelop, messageEnvelop2);

            // Act
            await changeHandler.Submitted(messageEnvelop);

            // Assert
            await database.Received(1).StoreSubmitted(EntityName.Contact, Arg.Is<Contact>(c => c.FirstName == "Apple" && c.LastName == "Orange"), "entity1", messageEnvelop.UpdateUser);
            await database.Received(2).GetEntityDocument(EntityName.Contact, messageEnvelop.EntityId);
            await auditManager.Received(1).Write(AuditTarget.Submitted, messageEnvelop2, messageEnvelop);
            await eventPublisher.Received(1).DataChangedAsync(messageEnvelop2);
        }

        [Fact]
        public async Task TakeEntityLock_WhenInvoked_TakesLockOnEntityId()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var changeHandler = new ChangeHandler(Substitute.For<ICustomerDatabase>(), distributedLock, Substitute.For<IEventPublisher>(), Substitute.For<IAuditManager>());
            var entityId = "entity1";
            distributedLock.Lock(entityId).Returns(TaskOutcome.OK);

            // Act
            var result = await changeHandler.TakeEntityLock(entityId);

            // Assert
            await distributedLock.Received(1).Lock(entityId);
            result.Successful.Should().BeTrue();
        }

        [Fact]
        public async Task TakeEntityLock_WhenLockIsNotAcquired_ReturnsFailure()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var changeHandler = new ChangeHandler(Substitute.For<ICustomerDatabase>(), distributedLock, Substitute.For<IEventPublisher>(), Substitute.For<IAuditManager>());
            var entityId = "entity1";
            distributedLock.Lock(entityId).Returns(TaskOutcome.LOCK_UNAVAILABLE);

            // Act
            var result = await changeHandler.TakeEntityLock(entityId);

            // Assert
            await distributedLock.Received(1).Lock(entityId);
            result.Successful.Should().BeFalse();
        }

        [Fact]
        public async Task ReleaseEntityLock_WhenInvoked_ReleasesLockOnEntityId()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var changeHandler = new ChangeHandler(Substitute.For<ICustomerDatabase>(), distributedLock, Substitute.For<IEventPublisher>(), Substitute.For<IAuditManager>());
            var entityId = "entity1";

            // Act
            await changeHandler.ReleaseEntityLock(entityId);

            // Assert
            await distributedLock.Received(1).Unlock(entityId);
        }

        [Fact]
        public async Task TryMergeDraft_ObtainsSpecialDraftLock_And_ThenAddsToDraftIfVersionsAreHigherAndAuditsAndPublishEvent()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var auditManager = Substitute.For<IAuditManager>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            var changeHandler = new ChangeHandler(database, distributedLock, eventPublisher, auditManager);
            var entityId = "entity1";
            var before = new MessageEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
            };
            var after = new MessageEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Cactus", LastName = "Mango" },
                DraftVersion = 5,
            };

            database.GetEntityDocument(EntityName.Contact, entityId).Returns(before, after);

            database.GetBasicInfo(EntityName.Contact, entityId).Returns(new EntityBasics { DraftVersion = 2 });

            // Act  
            var result = await changeHandler.TryMergeDraft(before);

            // Assert
            await distributedLock.Received(1).Lock($"{entityId}_draft");
            await database.Received(1).GetBasicInfo(EntityName.Contact, entityId);
            await database.Received(1).MergeDraft(before, before.DraftVersion + 1);
            await distributedLock.Received(1).Unlock($"{entityId}_draft");
            await auditManager.Received(1).Write(AuditTarget.Draft, after, before);
            await eventPublisher.Received(1).DataChangedAsync(after);
        }

        [Fact]
        public async Task TryMergeDraft_WhenDraftVersionDoNotMatch_ReturnsVersionMismatch()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, Substitute.For<IEventPublisher>(), Substitute.For<IAuditManager>());
            var entityId = "entity1";

            var messageEnvelop = new MessageEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 1,
            };

            database.GetBasicInfo(EntityName.Contact, entityId).Returns(new EntityBasics { DraftVersion = 2 });

            // Act  
            var result = await changeHandler.TryMergeDraft(messageEnvelop);

            // Assert
            await distributedLock.Received(1).Lock($"{entityId}_draft");
            await database.Received(1).GetBasicInfo(EntityName.Contact, entityId);
            await database.DidNotReceive().StoreDraft(messageEnvelop, messageEnvelop.DraftVersion + 1);
            await distributedLock.Received(1).Unlock($"{entityId}_draft");
            result.Should().Be(TaskOutcome.VERSION_MISMATCH);
        }

        [Fact]
        public async Task TryMergeDraft_WhenDeletedActionIsDetected_ReturnsVersionsAreNotChecksAndAudits()
        {
            // Arrange
            var auditManager = Substitute.For<IAuditManager>();
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, Substitute.For<IEventPublisher>(), auditManager);
            var entityId = "entity1";

            var before = new MessageEnvelop
            {
                Change = ChangeType.Delete,
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 5,
            };
            var after = new MessageEnvelop
            {
                Change = ChangeType.Delete,
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Green", LastName = "Black" },
                DraftVersion = 5,
            };

            var basics = new EntityBasics { DraftVersion = 5 };
            database.GetBasicInfo(EntityName.Contact, entityId).Returns(basics);
            database.GetEntityDocument(EntityName.Contact, entityId).Returns(before, after);

            // Act  
            var result = await changeHandler.TryMergeDraft(before);

            // Assert
            await distributedLock.Received(1).Lock($"{entityId}_draft");
            await database.Received(1).GetBasicInfo(EntityName.Contact, entityId);
            await database.Received(1).MergeDraft(before, basics.DraftVersion + 1);
            await distributedLock.Received(1).Unlock($"{entityId}_draft");
            await auditManager.Received(1).Write(AuditTarget.Draft, after, before);
        }

        [Fact]
        public async Task TryLockSubmitted_WhenBothDraftAndSubmittedVersionsAreTheSame_ThenDoesNotSubmit()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, Substitute.For<IEventPublisher>(), Substitute.For<IAuditManager>());
            var entityId = "entity1";

            var messageEnvelop = new MessageEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
                IsSubmitted = true,
                UpdateUser = "testUser"
            };

            var storedDraft = new Contact() { FirstName = "Apple", LastName = "Orange" };
            database.GetEntityDocument(EntityName.Contact, entityId).Returns(new MessageEnvelop()
            {
                Name = EntityName.Contact,
                EntityId = entityId,
                Draft = storedDraft,
                DraftVersion = 2,
                SubmittedVersion = 2,
            });

            distributedLock.Lock(entityId).Returns(TaskOutcome.OK);

            // Act  
            var result = await changeHandler.TryLockSubmitted(messageEnvelop);

            // Assert
            result.Should().Be(TaskOutcome.NO_CHANGE_TO_SUBMIT);
        }

        [Fact]
        public async Task TryLockSubmitted_WhenInvoked_ThenTakesEntityLockAndCopiesLatestDraftToSubmittedAndAuditsAndPublishEvent()
        {
            // Arrange
            var auditManager = Substitute.For<IAuditManager>();
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            var changeHandler = new ChangeHandler(database, distributedLock, eventPublisher, auditManager);
            var entityId = "entity1";

            var messageEnvelop = new MessageEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
                IsSubmitted = true,
                UpdateUser = "testUser"
            };

            var after = new MessageEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
                IsSubmitted = true,
                UpdateUser = "testUser"
            };

            var storedDraft = new Contact() { FirstName = "Apple", LastName = "Orange" };

            var before = new MessageEnvelop()
            {
                Name = EntityName.Contact,
                EntityId = entityId,
                Draft = storedDraft,
                DraftVersion = 2
            };

            database.GetEntityDocument(EntityName.Contact, entityId).Returns(before, after);

            distributedLock.Lock(entityId).Returns(TaskOutcome.OK);

            // Act  
            var result = await changeHandler.TryLockSubmitted(messageEnvelop);

            // Assert
            await distributedLock.Received(1).Lock(entityId);
            await database.Received(2).GetEntityDocument(EntityName.Contact, entityId);
            await database.Received(1).StoreSubmitted(EntityName.Contact, Arg.Is<Contact>(c => c == storedDraft), entityId, messageEnvelop.UpdateUser);
            await auditManager.Received(1).Write(AuditTarget.Submitted, after, before);
            await eventPublisher.Received(1).DataChangedAsync(after);
        }

        [Fact]
        public async Task ChangeStatusTo_WhenInvoked_ThenChangesStatusOfEntityDocumentAndPublishEvent()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, eventPublisher, Substitute.For<IAuditManager>());
            var entityId = "entity1";

            var messageEnvelop = new MessageEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
                IsSubmitted = true
            };
            messageEnvelop.SetState(EntityState.EVALUATING);

            database.GetEntityDocument(EntityName.Contact, entityId).Returns(messageEnvelop);
            var feedbacks = new Feedback() { Type = FeedbackType.Warning, Key = "PendingRiskChecks", Value = "Waiting" };

            // Act
            var result = await changeHandler.ChangeStatusTo(messageEnvelop.EntityId, messageEnvelop.Name, EntityState.IN_REVIEW, [feedbacks]);

            // Assert
            await database.Received(1).GetEntityDocument(EntityName.Contact, entityId);
            await database.Received(1).UpdateData(EntityName.Contact, entityId, EntityState.IN_REVIEW, Arg.Any<Feedback[]>(), Arg.Any<OrchestrationData[]>());
            await eventPublisher.Received(1).DataChangedAsync(messageEnvelop);
        }

        [Fact]
        public async Task ChangeStatusTo_WhenStateChangedToSynchronised_ThenCopiesSubmittedToAppliedAndAudits()
        {
            // Arrange
            var auditManager = Substitute.For<IAuditManager>();
            var distributedLock = Substitute.For<IDistributedLock>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, eventPublisher, auditManager);
            var entityId = "entity1";
            var before = new MessageEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
                IsSubmitted = true
            };
            var after = new MessageEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Gray", LastName = "Yellow" },
                DraftVersion = 2,
                IsSubmitted = true
            };

            before.SetState(EntityState.EVALUATING);

            database.GetEntityDocument(EntityName.Contact, entityId).Returns(before, after);

            // Act
            var result = await changeHandler.ChangeStatusTo(before.EntityId, before.Name, EntityState.SYNCHRONISED);

            // Assert
            await database.Received(1).StoreApplied(EntityName.Contact, Arg.Any<IEntity>(), entityId);
            await database.Received(1).UpdateData(EntityName.Contact, entityId, EntityState.SYNCHRONISED, Arg.Any<Feedback[]>(), Arg.Any<OrchestrationData[]>());
            await auditManager.Received(1).Write(AuditTarget.Applied, after, before);
        }
    }
}

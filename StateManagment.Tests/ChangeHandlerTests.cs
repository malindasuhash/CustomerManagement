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
        public void Draft_WhenEntityIsDrafted_StoresEntityUnderDraft()
        {
            // Arrange
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, Substitute.For<IDistributedLock>(), Substitute.For<IEventPublisher>());
            var messageEnvelop = new MessageEnvelop
            {
                EntityId = "entity1",
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" }
            };

            // Act
            changeHandler.Draft(messageEnvelop);

            // Assert
            database.Received(1).StoreDraft(messageEnvelop, messageEnvelop.DraftVersion + 1);
        }

        [Fact]
        public void Submitted_WhenEntityIsSubmitted_ThenStoresInDatastore()
        {
            // Arrange
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, Substitute.For<IDistributedLock>(), Substitute.For<IEventPublisher>());
            var messageEnvelop = new MessageEnvelop
            {
                EntityId = "entity1",
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 3,
                UpdateUser = "testUser"
            };

            // Act
            changeHandler.Submitted(messageEnvelop);

            // Assert
            database.Received(1).StoreSubmitted(EntityName.Contact, Arg.Is<Contact>(c => c.FirstName == "Apple" && c.LastName == "Orange"), "entity1", messageEnvelop.UpdateUser);
        }

        [Fact]
        public async Task TakeEntityLock_WhenInvoked_TakesLockOnEntityId()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var changeHandler = new ChangeHandler(Substitute.For<ICustomerDatabase>(), distributedLock, Substitute.For<IEventPublisher>());
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
            var changeHandler = new ChangeHandler(Substitute.For<ICustomerDatabase>(), distributedLock, Substitute.For<IEventPublisher>());
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
            var changeHandler = new ChangeHandler(Substitute.For<ICustomerDatabase>(), distributedLock, Substitute.For<IEventPublisher>());
            var entityId = "entity1";

            // Act
            await changeHandler.ReleaseEntityLock(entityId);

            // Assert
            await distributedLock.Received(1).Unlock(entityId);
        }

        [Fact]
        public async Task TryMergeDraft_ObtainsSpecialDraftLock_And_ThenAddsToDraftIfVersionsAreHigher()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, Substitute.For<IEventPublisher>());
            var entityId = "entity1";
            var messageEnvelop = new MessageEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
            };

            database.GetBasicInfo(EntityName.Contact, entityId).Returns(new EntityBasics { DraftVersion = 2 });

            // Act  
            var result = await changeHandler.TryMergeDraft(messageEnvelop);

            // Assert
            await distributedLock.Received(1).Lock($"{entityId}_draft");
            await database.Received(1).GetBasicInfo(EntityName.Contact, entityId);
            await database.Received(1).MergeDraft(messageEnvelop, messageEnvelop.DraftVersion + 1);
            await distributedLock.Received(1).Unlock($"{entityId}_draft");
        }

        [Fact]
        public async Task TryMergeDraft_WhenDraftVersionDoNotMatch_ReturnsVersionMismatch()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, Substitute.For<IEventPublisher>());
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
        public async Task TryMergeDraft_WhenDeletedActionIsDetected_ReturnsVersionsAreNotChecks()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, Substitute.For<IEventPublisher>());
            var entityId = "entity1";

            var messageEnvelop = new MessageEnvelop
            {
                Change = ChangeType.Delete,
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 5,
            };

            var basics = new EntityBasics { DraftVersion = 5 };
            database.GetBasicInfo(EntityName.Contact, entityId).Returns(basics);

            // Act  
            var result = await changeHandler.TryMergeDraft(messageEnvelop);

            // Assert
            await distributedLock.Received(1).Lock($"{entityId}_draft");
            await database.Received(1).GetBasicInfo(EntityName.Contact, entityId);
            await database.Received(1).MergeDraft(messageEnvelop, basics.DraftVersion + 1);
            await distributedLock.Received(1).Unlock($"{entityId}_draft");
            result.Should().Be(TaskOutcome.OK);
        }

        [Fact]
        public async Task TryLockSubmitted_WhenBothDraftAndSubmittedVersionsAreTheSame_ThenDoesNotSubmit()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, Substitute.For<IEventPublisher>());
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
        public async Task TryLockSubmitted_WhenInvoked_ThenTakesEntityLockAndCopiesLatestDraftToSubmitted()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, Substitute.For<IEventPublisher>());
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
                DraftVersion = 2
            });

            distributedLock.Lock(entityId).Returns(TaskOutcome.OK);

            // Act  
            var result = await changeHandler.TryLockSubmitted(messageEnvelop);

            // Assert
            await distributedLock.Received(1).Lock(entityId);
            await database.Received(1).GetEntityDocument(EntityName.Contact, entityId);
            await database.Received(1).StoreSubmitted(EntityName.Contact, Arg.Is<Contact>(c => c == storedDraft), entityId, messageEnvelop.UpdateUser);
        }

        [Fact]
        public async Task ChangeStatusTo_WhenInvoked_ThenChangesStatusOfEntityDocumentAndPublishEvent()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, eventPublisher);
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

            // Act
            var result = await changeHandler.ChangeStatusTo(messageEnvelop.EntityId, messageEnvelop.Name, EntityState.IN_REVIEW, ["PendingRiskChesks"]);

            // Assert
            database.Received(1).GetEntityDocument(EntityName.Contact, entityId);
            database.Received(1).UpdateData(EntityName.Contact, entityId, EntityState.IN_REVIEW, Arg.Is<string[]>(s => s.Contains("PendingRiskChesks")));
            await eventPublisher.Received(1).PublishStateChangedEvent(messageEnvelop);
        }

        [Fact]
        public async Task ChangeStatusTo_WhenStateChangedToSynchronised_ThenCopiesSubmittedToApplied()
        {
                       // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, eventPublisher);
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

            // Act
            var result = await changeHandler.ChangeStatusTo(messageEnvelop.EntityId, messageEnvelop.Name, EntityState.SYNCHRONISED);

            // Assert
            database.Received(1).StoreApplied(EntityName.Contact, Arg.Any<IEntity>(), entityId);
            database.Received(1).UpdateData(EntityName.Contact, entityId, EntityState.SYNCHRONISED, Arg.Is<string[]>(s => s.Length == 0));
        }
    }
}

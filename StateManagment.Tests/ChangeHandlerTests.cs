using FluentAssertions;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;

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
                CustomerId = "customer1",
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" }
            };
            var searchBy = messageEnvelop.SearchBy();
            database.FindEntity<Contact>(searchBy).Returns(messageEnvelop);

            // Act
            await changeHandler.Draft<Contact>(messageEnvelop);

            // Assert
            await database.Received(1).StoreDraft<Contact>(messageEnvelop, messageEnvelop.DraftVersion + 1);
            await database.Received(1).FindEntity<Contact>(searchBy);
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
            var customerId = "customer1";

            var messageEnvelop = new MessageEnvelop
            {
                EntityId = "entity1",
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 3,
                UpdateUser = "testUser"
            };

            var messageEnvelop2 = new MessageEnvelop
            {
                EntityId = "entity1",
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Pink", LastName = "Blue" },
                DraftVersion = 3,
                UpdateUser = "testUser"
            };

            database.FindEntity<Contact>(messageEnvelop.SearchBy()).Returns(messageEnvelop, messageEnvelop2);

            // Act
            await changeHandler.Submitted<Contact>(messageEnvelop);

            // Assert
            await database.Received(1).StoreSubmitted<Contact>(Arg.Is<LookupPredicate>(p => p.EntityId.Equals("entity1") && p.CustomerId.Equals(customerId)), messageEnvelop.UpdateUser);
            await database.Received(2).FindEntity<Contact>(Arg.Is<LookupPredicate>(p => p.EntityId.Equals("entity1")));
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
            var customerId = "customer1";
            var before = new MessageEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
            };
            var after = new MessageEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Cactus", LastName = "Mango" },
                DraftVersion = 5,
            };

            var searchBy = after.SearchBy();

            database.FindEntity<Contact>(searchBy).Returns(before, after);

            database.GetBasicInfo<Contact>(before.SearchBy()).Returns(new EntityBasics { DraftVersion = 2 });

            // Act  
            var result = await changeHandler.TryMergeDraft<Contact>(before);

            // Assert
            await distributedLock.Received(1).Lock($"{entityId}_draft");
            await database.Received(1).GetBasicInfo<Contact>(Arg.Is<LookupPredicate>(p => p.EntityId.Equals(entityId)));
            await database.Received(1).MergeDraft<Contact>(before, before.DraftVersion + 1);
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
            var customerId = "customer1";
            var messageEnvelop = new MessageEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 1,
            };

            database.GetBasicInfo<Contact>(messageEnvelop.SearchBy()).Returns(new EntityBasics { DraftVersion = 2 });

            // Act  
            var result = await changeHandler.TryMergeDraft<Contact>(messageEnvelop);

            // Assert
            await distributedLock.Received(1).Lock($"{entityId}_draft");
            await database.Received(1).GetBasicInfo<Contact>(Arg.Is<LookupPredicate>(p => p.EntityId.Equals(entityId)));
            await database.DidNotReceive().StoreDraft<Contact>(messageEnvelop, messageEnvelop.DraftVersion + 1);
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
            var customerId = "customer1";
            var before = new MessageEnvelop
            {
                Change = ChangeType.Delete,
                EntityId = entityId,
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 5,
            };
            var after = new MessageEnvelop
            {
                Change = ChangeType.Delete,
                EntityId = entityId,
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Green", LastName = "Black" },
                DraftVersion = 5,
            };

            var searchBy = after.SearchBy();

            var basics = new EntityBasics { DraftVersion = 5 };
            database.GetBasicInfo<Contact>(before.SearchBy()).Returns(basics);
            database.FindEntity<Contact>(searchBy).Returns(before, after);

            // Act  
            var result = await changeHandler.TryMergeDraft<Contact>(before);

            // Assert
            await distributedLock.Received(1).Lock($"{entityId}_draft");
            await database.Received(1).GetBasicInfo<Contact>(Arg.Is<LookupPredicate>(p => p.EntityId.Equals(entityId)));
            await database.Received(1).MergeDraft<Contact>(before, basics.DraftVersion + 1);
            await distributedLock.Received(1).Unlock($"{entityId}_draft");
            await auditManager.Received(1).Write(AuditTarget.Draft, after, before);
        }

        [Fact]
        public async Task TryLockSubmitted_WhenSubmittedChangeIsReceived_ThenGivenDraftVersionMustMatchBeforeProcessing()
        {

            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, Substitute.For<IEventPublisher>(), Substitute.For<IAuditManager>());
            var entityId = "entity1";
            var customerId = "customer1";
            var stored = new MessageEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
                IsSubmitted = true,
                UpdateUser = "testUser",
                Change = ChangeType.Submit
            };

            var received = new MessageEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 4,
                IsSubmitted = true,
                UpdateUser = "testUser",
                Change = ChangeType.Submit
            };

            var searchBy = stored.SearchBy();

            database.FindEntity<Contact>(searchBy).Returns(stored);

            // Act  
            var result = await changeHandler.TryLockSubmitted<Contact>(received);

            // Assert
            result.Should().Be(TaskOutcome.VERSION_MISMATCH);
        }

        [Fact]
        public async Task TryLockSubmitted_WhenBothDraftAndSubmittedVersionsAreTheSame_ThenDoesNotSubmit()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, Substitute.For<IEventPublisher>(), Substitute.For<IAuditManager>());
            var entityId = "entity1";
            var customerId = "customer1";
            var messageEnvelop = new MessageEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
                IsSubmitted = true,
                UpdateUser = "testUser"
            };

            var searchBy = messageEnvelop.SearchBy();

            var storedDraft = new Contact() { FirstName = "Apple", LastName = "Orange" };
            database.FindEntity<Contact>(searchBy).Returns(new MessageEnvelop()
            {
                Name = EntityName.Contact,
                EntityId = entityId,
                CustomerId= customerId,
                Draft = storedDraft,
                DraftVersion = 2,
                SubmittedVersion = 2,
            });

            distributedLock.Lock(entityId).Returns(TaskOutcome.OK);

            // Act  
            var result = await changeHandler.TryLockSubmitted<Contact>(messageEnvelop);

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
            var customerId = "customerId1";

            var messageEnvelop = new MessageEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
                IsSubmitted = true,
                UpdateUser = "testUser"
            };

            var after = new MessageEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
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
                CustomerId = customerId,
                Draft = storedDraft,
                DraftVersion = 2
            };

            var searchBy = messageEnvelop.SearchBy();

            database.FindEntity<Contact>(searchBy).Returns(before, after);

            distributedLock.Lock(entityId).Returns(TaskOutcome.OK);

            // Act  
            var result = await changeHandler.TryLockSubmitted<Contact>(messageEnvelop);

            // Assert
            await distributedLock.Received(1).Lock(entityId);
            await database.Received(2).FindEntity<Contact>(searchBy);
            await database.Received(1).StoreSubmitted<Contact>(Arg.Is<LookupPredicate>(p => p.EntityId.Equals(entityId) && p.CustomerId.Equals(customerId)), messageEnvelop.UpdateUser);
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
            var customerId = "customerId1";

            var messageEnvelop = new MessageEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
                IsSubmitted = true
            };
            messageEnvelop.SetState(EntityState.EVALUATING);
            var searchBy = messageEnvelop.SearchBy();
            database.FindEntity<Contact>(searchBy).Returns(messageEnvelop);
            var feedbacks = new Feedback() { Type = FeedbackType.Warning, Key = "PendingRiskChecks", Value = "Waiting" };

            // Act
            var result = await changeHandler.ChangeStatusTo<Contact>(messageEnvelop.SearchBy(), EntityState.IN_REVIEW, [feedbacks]);

            // Assert
            await database.Received(1).FindEntity<Contact>(searchBy);
            await database.Received(1).UpdateData<Contact>(Arg.Is<LookupPredicate>(p => p.EntityId.Equals(entityId) && p.CustomerId.Equals(customerId)), EntityState.IN_REVIEW, Arg.Any<Feedback[]>(), Arg.Any<OrchestrationData[]>());
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
            var customerId = "customerId1";

            var before = new MessageEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
                IsSubmitted = true,
                RemoveRequested = true
            };
            var after = new MessageEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Gray", LastName = "Yellow" },
                DraftVersion = 2,
                IsSubmitted = true
            };

            before.SetState(EntityState.EVALUATING);
            var searchBy = before.SearchBy();
            database.FindEntity<Contact>(searchBy).Returns(before, after);

            // Act
            var result = await changeHandler.ChangeStatusTo<Contact>(searchBy, EntityState.SYNCHRONISED);

            // Assert
            await database.Received(1).StoreApplied<Contact>(Arg.Is<LookupPredicate>(p => p.EntityId.Equals(entityId) && p.CustomerId.Equals(customerId)), before.RemoveRequested);
            await database.Received(1).UpdateData<Contact>(Arg.Is<LookupPredicate>(p => p.EntityId.Equals(entityId) && p.CustomerId.Equals(customerId)), EntityState.SYNCHRONISED, Arg.Any<Feedback[]>(), Arg.Any<OrchestrationData[]>());
            await auditManager.Received(1).Write(AuditTarget.Applied, after, before);
        }

        [Fact]
        public async Task TryMarkForRemoval_WhenInvoked_RemovalHappensWithinAnEntityLock()
        {
            // Arrange
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.None,
                Name = EntityName.Contact,
                EntityId = "123",
                CustomerId = "342",
                DraftVersion = 2,
                SubmittedVersion = 1,
                IsSubmitted = true
            };

            var auditManager = Substitute.For<IAuditManager>();
            var distributedLock = Substitute.For<IDistributedLock>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock, eventPublisher, auditManager);

            // Act
            await changeHandler.TryMarkForRemoval<Contact>(envelop);

            // Assert
            await distributedLock.Received(1).Lock(envelop.EntityId);
            await distributedLock.Received(1).Unlock(envelop.EntityId);
        }

        [Fact]
        public async Task TryMarkForRemoval_RemovalsItemFromDatabase_AuditsAndPublishedEvent()
        {
            // Arrange
            var before = new MessageEnvelop
            {
                Change = ChangeType.None,
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 2,
                SubmittedVersion = 1,
                IsSubmitted = true,
                CustomerId = "123",
            };
            var after = new MessageEnvelop
            {
                Change = ChangeType.None,
                Name = EntityName.Contact,
                EntityId = "123",
                DraftVersion = 2,
                SubmittedVersion = 1,
                IsSubmitted = true,
                CustomerId = "1234"
            };

            var database = Substitute.For<ICustomerDatabase>();
            database.FindEntity<Contact>(Arg.Any<LookupPredicate>()).Returns(before, after);

            var auditManager = Substitute.For<IAuditManager>();
            var distributedLock = Substitute.For<IDistributedLock>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            
            var changeHandler = new ChangeHandler(database, distributedLock, eventPublisher, auditManager);

            // Act
            await changeHandler.TryMarkForRemoval<Contact>(before);

            // Assert
            await database.Received(1).MarkForRemoval<Contact>(Arg.Is<LookupPredicate>(p => p.EntityId.Equals("123") && p.CustomerId.Equals("1234")));
            await auditManager.Received(1).Write(AuditTarget.Document, after, before);
            await eventPublisher.Received(1).DataChangedAsync(after);
        }
    }
}

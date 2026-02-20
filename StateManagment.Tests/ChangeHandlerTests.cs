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
            var changeHandler = new ChangeHandler(database, Substitute.For<IDistributedLock>());
            var messageEnvelop = new MessageEnvelop
            {
                EntityId = "entity1",
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" }
            };

            // Act
            changeHandler.Draft(messageEnvelop);

            // Assert
            database.Received(1).StoreDraft(EntityName.Contact, Arg.Is<Contact>(c => c.FirstName == "Apple" && c.LastName == "Orange"), messageEnvelop.EntityId);
        }

        [Fact]
        public void Submitted_WhenEntityIsSubmitted_ThenStoresInDatastore()
        {
            // Arrange
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, Substitute.For<IDistributedLock>());
            var messageEnvelop = new MessageEnvelop
            {
                EntityId = "entity1",
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" }
            };

            // Act
            changeHandler.Submitted(messageEnvelop);

            // Assert
            database.Received(1).StoreSubmitted(EntityName.Contact, Arg.Is<Contact>(c => c.FirstName == "Apple" && c.LastName == "Orange"), "entity1");
        }

        [Fact]
        public async Task TakeEntityLock_WhenInvoked_TakesLockOnEntityId()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var changeHandler = new ChangeHandler(Substitute.For<ICustomerDatabase>(), distributedLock);
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
            var changeHandler = new ChangeHandler(Substitute.For<ICustomerDatabase>(), distributedLock);
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
            var changeHandler = new ChangeHandler(Substitute.For<ICustomerDatabase>(), distributedLock);
            var entityId = "entity1";

            // Act
            await changeHandler.ReleaseEntityLock(entityId);

            // Assert
            await distributedLock.Received(1).Unlock(entityId);
        }

        [Fact]
        public async Task TryDraft_ObtainsSpecialDraftLock_And_ThenAddsToDraftIfVersionsAreHigher()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock);
            var entityId = "entity1";
            var messageEnvelop = new MessageEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                Draft = new Contact() { FirstName = "Apple", LastName = "Orange" },
                DraftVersion = 2,
            };

            database.GetBasicInfo(EntityName.Contact, entityId).Returns(new EntityBasics { DraftVersion = 1 });

            // Act  
            var result = await changeHandler.TryDraft(messageEnvelop);

            // Assert
            await distributedLock.Received(1).Lock($"{entityId}_draft");
            database.Received(1).GetBasicInfo(EntityName.Contact, entityId);
            database.Received(1).StoreDraft(EntityName.Contact, Arg.Is<Contact>(c => c.FirstName == "Apple" && c.LastName == "Orange"), entityId);
            await distributedLock.Received(1).Unlock($"{entityId}_draft");
        }

        [Fact]
        public async Task TryDraft_WhenDraftVersionIsStale_ReturnsStaleDraft()
        {
            // Arrange
            var distributedLock = Substitute.For<IDistributedLock>();
            var database = Substitute.For<ICustomerDatabase>();
            var changeHandler = new ChangeHandler(database, distributedLock);
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
            var result = await changeHandler.TryDraft(messageEnvelop);

            // Assert
            await distributedLock.Received(1).Lock($"{entityId}_draft");
            database.Received(1).GetBasicInfo(EntityName.Contact, entityId);
            database.DidNotReceive().StoreDraft(Arg.Any<EntityName>(), Arg.Any<Contact>(), Arg.Any<string>());
            await distributedLock.Received(1).Unlock($"{entityId}_draft");
            result.Should().Be(TaskOutcome.STALE_DRAFT);
        }
    }
}

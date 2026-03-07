using FluentAssertions;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;
using StateManagment.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Tests.Services
{
    public class ContactServiceTests
    {
        [Fact]
        public async Task Touch_WhenInvoked_ThenDispatchTouchRequest()
        {
            // Arrange
            var changeProcessor = Substitute.For<IChangeProcessor>();
            var customerDatabase = Substitute.For<ICustomerDatabase>();
            var contactService = new ContactService(changeProcessor, customerDatabase);
            var entityId = "EntityId";
            changeProcessor.ProcessChangeAsync(Arg.Any<MessageEnvelop>()).Returns(TaskOutcome.LOCK_UNAVAILABLE);

            // Act
            var result = await contactService.Touch(entityId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync(Arg.Is<MessageEnvelop>(a => a.EntityId.Equals(entityId) && a.Name == EntityName.Contact && a.Change == ChangeType.Touch));
            result.Should().Be(TaskOutcome.LOCK_UNAVAILABLE);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Post_IndicatesCreation_ANewContact(bool submit)
        {
            // Arrange
            var changeProcessor = Substitute.For<IChangeProcessor>();
            var customerDatabase = Substitute.For<ICustomerDatabase>();
            customerDatabase.GetEntityDocument(Arg.Any<EntityName>(), Arg.Any<string>()).Returns(Task.FromResult(new MessageEnvelop
            {
                Change = ChangeType.Read,
                Name = EntityName.Contact,
                EntityId = "321",
                IsSubmitted = submit
            }));
            changeProcessor.ProcessChangeAsync(Arg.Any<MessageEnvelop>()).Returns(a => { ((MessageEnvelop)a[0]).EntityId = "321"; return Task.FromResult(TaskOutcome.OK); });
            var contactService = new ContactService(changeProcessor, customerDatabase);
            var customerId = "cus123";

            // Act
            var result = await contactService.Post(customerId, new Contact { FirstName = "John", LastName = "Doe" }, submit);

            // Assert
            result.Change.Should().Be(ChangeType.Read);
            result.Name.Should().Be(EntityName.Contact);
            result.EntityId.Should().Be("321");
            _ = await changeProcessor.Received(1).ProcessChangeAsync(Arg.Is<MessageEnvelop>(e =>

                e.Change == ChangeType.Create && e.Name == EntityName.Contact && e.IsSubmitted == submit
                && e.CustomerId == customerId

            ));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Patch_WhenInvoked_ThenUpdatesDraftInstance(bool submit)
        {
            // Arrange
            var changeProcessor = Substitute.For<IChangeProcessor>();
            var customerDatabase = Substitute.For<ICustomerDatabase>();
            var contact = new Contact { FirstName = "John", LastName = "Doe" };
            var customerId = "CustomerId";
            var targetVersion = 2;

            customerDatabase.GetEntityDocument(Arg.Any<EntityName>(), Arg.Any<string>()).Returns(Task.FromResult(new MessageEnvelop
            {
                Change = ChangeType.Read,
                Name = EntityName.Contact,
                EntityId = "321",
                IsSubmitted = submit,
                CustomerId = customerId
            }));

            changeProcessor.ProcessChangeAsync(Arg.Any<MessageEnvelop>()).Returns(a => { ((MessageEnvelop)a[0]).EntityId = "321"; return Task.FromResult(TaskOutcome.OK); });
            var contactService = new ContactService(changeProcessor, customerDatabase);

            // Act
            var result = await contactService.Patch(contact, customerId, "321", targetVersion, submit);

            // Assert   
            result.Change.Should().Be(ChangeType.Read);
            result.Name.Should().Be(EntityName.Contact);
            result.EntityId.Should().Be("321");
            _ = await changeProcessor.Received(1).ProcessChangeAsync(Arg.Is<MessageEnvelop>(e =>

                e.Change == ChangeType.Update && e.Name == EntityName.Contact && e.IsSubmitted == submit && e.EntityId == "321" 
                && e.DraftVersion == targetVersion

            ));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Delete_WhenInvoked_ThenDelegatesToChangeProcessor(bool submit)
        {
            // Arrange
            var changeProcessor = Substitute.For<IChangeProcessor>();
            var customerDatabase = Substitute.For<ICustomerDatabase>();
            var contact = new Contact { FirstName = "John", LastName = "Doe" };
            var customerId = "customerId";

            customerDatabase.GetEntityDocument(Arg.Any<EntityName>(), Arg.Any<string>()).Returns(Task.FromResult(new MessageEnvelop
            {
                Change = ChangeType.Read,
                Name = EntityName.Contact,
                EntityId = "321",
                IsSubmitted = submit,
                CustomerId = customerId,
            }));

            changeProcessor.ProcessChangeAsync(Arg.Any<MessageEnvelop>()).Returns(a => { ((MessageEnvelop)a[0]).EntityId = "321"; return Task.FromResult(TaskOutcome.OK); });
            var contactService = new ContactService(changeProcessor, customerDatabase);

            // Act
            var result = await contactService.Delete(customerId, "321", submit);

            // Assert
            result.Successful.Should().BeTrue();
            _ = await changeProcessor.Received(1).ProcessChangeAsync(Arg.Is<MessageEnvelop>(e =>

               e.Change == ChangeType.Delete && e.Name == EntityName.Contact && e.IsSubmitted == submit && e.EntityId == "321"
               && e.CustomerId == customerId

           ));
        }

        [Fact]
        public async Task Get_WhenInvoked_ReturnsEntityDocument()
        {
            // Arrange
            var changeProcessor = Substitute.For<IChangeProcessor>();
            var customerDatabase = Substitute.For<ICustomerDatabase>();
            customerDatabase.GetEntityDocument(Arg.Any<EntityName>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(new MessageEnvelop
            {
                Change = ChangeType.Read,
                Name = EntityName.Contact,
                EntityId = "321",
                IsSubmitted = false,
                CustomerId = "CustomerId"
            }));

            var contactService = new ContactService(changeProcessor, customerDatabase);

            // Act
            var result = await contactService.Get("CustomerId", "321");

            // Assert   
            result.Change.Should().Be(ChangeType.Read);
            result.Name.Should().Be(EntityName.Contact);
            result.EntityId.Should().Be("321");
            result.CustomerId.Should().Be("CustomerId");
            _ = await customerDatabase.Received(1).GetEntityDocument(Arg.Is<EntityName>(n => n == EntityName.Contact), Arg.Is<string>(s => s == "321"), Arg.Is<string>(a => a.Equals("CustomerId")));
        }
    }
}

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
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Post_IndicatesCreation_ANewContact(bool submit)
        {
            // Arrange
            var changeProcessor = Substitute.For<IChangeProcessor>();
            var dataRetriver = Substitute.For<IDataRetriever>();
            dataRetriver.GetEntityEnvelop(Arg.Any<string>(), Arg.Any<EntityName>()).Returns(Task.FromResult(new MessageEnvelop
            {
                Change = ChangeType.Read,
                Name = EntityName.Contact,
                EntityId = "321",
                IsSubmitted = submit
            }));
            changeProcessor.ProcessChangeAsync(Arg.Any<MessageEnvelop>()).Returns(a => { ((MessageEnvelop)a[0]).EntityId = "321"; return Task.FromResult(TaskOutcome.OK); });
            var contactService = new ContactService(changeProcessor, dataRetriver);

            // Act
            var result = await contactService.Post(new Contact { FirstName = "John", LastName = "Doe" }, submit);

            // Assert
            result.Change.Should().Be(ChangeType.Read);
            result.Name.Should().Be(EntityName.Contact);
            result.EntityId.Should().Be("321");
            _ = await changeProcessor.Received(1).ProcessChangeAsync(Arg.Is<MessageEnvelop>(e =>

                e.Change == ChangeType.Create && e.Name == EntityName.Contact && e.IsSubmitted == submit

            ));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Patch_WhenInvoked_ThenUpdatesDraftInstance(bool submit)
        {
            // Arrange
            var changeProcessor = Substitute.For<IChangeProcessor>();
            var dataRetriver = Substitute.For<IDataRetriever>();
            var contact = new Contact { FirstName = "John", LastName = "Doe" };

            dataRetriver.GetEntityEnvelop(Arg.Any<string>(), Arg.Any<EntityName>()).Returns(Task.FromResult(new MessageEnvelop
            {
                Change = ChangeType.Read,
                Name = EntityName.Contact,
                EntityId = "321",
                IsSubmitted = submit
            }));

            changeProcessor.ProcessChangeAsync(Arg.Any<MessageEnvelop>()).Returns(a => { ((MessageEnvelop)a[0]).EntityId = "321"; return Task.FromResult(TaskOutcome.OK); });
            var contactService = new ContactService(changeProcessor, dataRetriver);

            // Act
            var result = await contactService.Patch(contact, "321", submit);

            // Assert   
            result.Change.Should().Be(ChangeType.Read);
            result.Name.Should().Be(EntityName.Contact);
            result.EntityId.Should().Be("321");
            _ = await changeProcessor.Received(1).ProcessChangeAsync(Arg.Is<MessageEnvelop>(e =>

                e.Change == ChangeType.Update && e.Name == EntityName.Contact && e.IsSubmitted == submit && e.EntityId == "321"

            ));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Delete_WhenInvoked_ThenDelegatesToChangeProcessor(bool submit)
        {
            // Arrange
            var changeProcessor = Substitute.For<IChangeProcessor>();
            var dataRetriver = Substitute.For<IDataRetriever>();
            var contact = new Contact { FirstName = "John", LastName = "Doe" };

            dataRetriver.GetEntityEnvelop(Arg.Any<string>(), Arg.Any<EntityName>()).Returns(Task.FromResult(new MessageEnvelop
            {
                Change = ChangeType.Read,
                Name = EntityName.Contact,
                EntityId = "321",
                IsSubmitted = submit
            }));

            changeProcessor.ProcessChangeAsync(Arg.Any<MessageEnvelop>()).Returns(a => { ((MessageEnvelop)a[0]).EntityId = "321"; return Task.FromResult(TaskOutcome.OK); });
            var contactService = new ContactService(changeProcessor, dataRetriver);

            // Act
            var result = await contactService.Delete("321", submit);

            // Assert
            result.Successful.Should().BeTrue();
            _ = await changeProcessor.Received(1).ProcessChangeAsync(Arg.Is<MessageEnvelop>(e =>

               e.Change == ChangeType.Delete && e.Name == EntityName.Contact && e.IsSubmitted == submit && e.EntityId == "321"

           ));
        }

        [Fact]
        public async Task Get_WhenInvoked_ReturnsEntityDocument()
        {             
            // Arrange
            var changeProcessor = Substitute.For<IChangeProcessor>();
            var dataRetriver = Substitute.For<IDataRetriever>();
            dataRetriver.GetEntityEnvelop(Arg.Any<string>(), Arg.Any<EntityName>()).Returns(Task.FromResult(new MessageEnvelop
            {
                Change = ChangeType.Read,
                Name = EntityName.Contact,
                EntityId = "321",
                IsSubmitted = false
            }));

            var contactService = new ContactService(changeProcessor, dataRetriver);

            // Act
            var result = await contactService.Get("321");

            // Assert   
            result.Change.Should().Be(ChangeType.Read);
            result.Name.Should().Be(EntityName.Contact);
            result.EntityId.Should().Be("321");
            _ = await dataRetriver.Received(1).GetEntityEnvelop(Arg.Is<string>(s => s == "321"), Arg.Is<EntityName>(n => n == EntityName.Contact));
        }
    }
}

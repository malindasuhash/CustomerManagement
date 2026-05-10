using Api.ApiModels;
using Api.Controllers;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Tests.Controllers
{
    public class ContactControllerTests
    {
        const string CustomerId = "CustomerId1";
        const string ContactId = "ContactId1";

        private readonly ContactController contactController;
        private readonly IChangeProcessor changeProcessor;
        private readonly ICustomerDatabase customerDatabase;

        public ContactControllerTests()
        {
            changeProcessor = Substitute.For<IChangeProcessor>();
            customerDatabase = Substitute.For<ICustomerDatabase>();
            customerDatabase.FindEntity<BankAccount>(Arg.Any<LookupPredicate>()).Returns(new MessageEnvelop() { CustomerId = CustomerId });
            contactController = new ContactController(changeProcessor, customerDatabase);
        }

        [Fact]
        public async Task TouchContact_WhenInvokedThen_ExpectedEnvelopIsCreated()
        {
            // Act
            await contactController.TouchContact(CustomerId, ContactId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<Contact>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(ContactId) && p.Change == ChangeType.Touch && p.Name == EntityName.Contact && p.CustomerId.Equals(CustomerId)));
        }

        [Fact]
        public async Task SubmitContact_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await contactController.SubmitContact(CustomerId, ContactId, new ApiContract.SubmitActionRequest() { Draft_version = 10 });

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<Contact>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(ContactId) && p.Change == ChangeType.Submit && p.Name == EntityName.Contact && p.CustomerId.Equals(CustomerId) && p.DraftVersion.Equals(10)));
        }

        [Fact]
        public async Task RemoveContact_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await contactController.RemoveContact(CustomerId, ContactId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<Contact>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(ContactId) && p.Change == ChangeType.Delete && p.Name == EntityName.Contact && p.CustomerId.Equals(CustomerId)));
        }

        [Fact]
        public async Task CreateContact_WhenInvoked_ThenUseAccurateCommand()
        {
            var contact = new ApiContract.CreateUpdateContact();

            // Act
            await contactController.CreateContact(CustomerId, contact);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<Contact>(Arg.Is<MessageEnvelop>(p => p.Change == ChangeType.Create && p.Name == EntityName.Contact && p.CustomerId.Equals(CustomerId)));
        }

        [Fact]
        public async Task GetContactById_WhenInvoked_ThenUseAccurateCommand()
        {
            // Arrange
            customerDatabase.FindEntity<Contact>(Arg.Any<LookupPredicate>()).Returns(new MessageEnvelop() { CustomerId = CustomerId });

            // Act
            await contactController.GetContactById(CustomerId, ContactId);

            // Assert
            await customerDatabase.Received(1).FindEntity<Contact>(Arg.Is<LookupPredicate>(p => p.CustomerId.Equals(CustomerId) && p.EntityId.Equals(ContactId)));
        }

        [Fact]
        public async Task UpdateContact_WhenUpdating_ThenIssuesTheAppropriateCommand()
        {
            // Arrange
            var patchModel = new ContactModel()
            {
                Email = "e@mail.com",
                Label = "label",
                TargetVersion = 20
            };

            // Act
            await contactController.UpateContact(CustomerId, ContactId, patchModel);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<Contact>(Arg.Is<MessageEnvelop>(m => SameAfterMapped(patchModel, m)));
        }

        private static bool SameAfterMapped(ContactModel contactModel, MessageEnvelop messageEnvelop)
        {
            var contactMapped = messageEnvelop.Draft as Contact;

            return messageEnvelop.Name == EntityName.Contact
                && messageEnvelop.Change == ChangeType.Update
                && messageEnvelop.DraftVersion == 20
                && contactMapped.Email == contactModel.Email;
        }
    }
}

using Api.ApiModels;
using Api.Controllers;
using FluentAssertions;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Tests
{
    public class BankAccountControllerTests
    {
        const string LegalEntityId = "LegalEntityId1";
        const string CustomerId = "CustomerId1";
        const string BankAccountId = "BankAccountId1";

        private readonly BankAccountController bankAccountController;
        private readonly IChangeProcessor changeProcessor;
        private readonly ICustomerDatabase customerDatabase;


        public BankAccountControllerTests()
        {
            changeProcessor = Substitute.For<IChangeProcessor>();
            customerDatabase = Substitute.For<ICustomerDatabase>();
            customerDatabase.FindEntity<BankAccount>(Arg.Any<LookupPredicate>()).Returns(new MessageEnvelop() { CustomerId = CustomerId});
            bankAccountController = new BankAccountController(changeProcessor, customerDatabase);
        }

        [Fact]
        public async Task TouchBankAccount_WhenInvokedThen_ExpectedEnvelopIsCreated()
        {
            // Act
            await bankAccountController.TouchBankAccount(CustomerId, LegalEntityId, BankAccountId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<BankAccount>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(BankAccountId) && p.Change == ChangeType.Touch && p.Name == EntityName.BankAccount && p.CustomerId.Equals(CustomerId) && LegalEntityIdCheck(p, LegalEntityId)));
        }

        [Fact]
        public async Task SubmitBankAccount_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await bankAccountController.SubmitBankAccount(CustomerId, LegalEntityId, BankAccountId, new SubmitEntityModel() { TargetVersion = 10 });

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<BankAccount>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(BankAccountId) && p.Change == ChangeType.Submit && p.Name == EntityName.BankAccount && p.CustomerId.Equals(CustomerId) && LegalEntityIdCheck(p, LegalEntityId) && p.DraftVersion.Equals(10)));
        }

        [Fact]
        public async Task RemoveBankAccount_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await bankAccountController.RemoveBankAccount(CustomerId, LegalEntityId, BankAccountId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<BankAccount>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(BankAccountId) && p.Change == ChangeType.Delete && p.Name == EntityName.BankAccount && p.CustomerId.Equals(CustomerId) && LegalEntityIdCheck(p, LegalEntityId)));
        }

        [Fact]
        public async Task CreateBankAccount_WhenInvoked_ThenUseAccurateCommand()
        {
            var bankAccount = new BankAccount();

            // Act
            await bankAccountController.CreateBankAccount(CustomerId, LegalEntityId, bankAccount);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<BankAccount>(Arg.Is<MessageEnvelop>(p => p.CustomerId.Equals(CustomerId) && SameDraft(bankAccount, p) && LegalEntityIdCheck(p, LegalEntityId)));
        }

        [Fact]
        public async Task GetBankAccountById_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await bankAccountController.GetBankAccountById(CustomerId, LegalEntityId, BankAccountId);

            // Assert
            await customerDatabase.Received(1).FindEntity<BankAccount>(Arg.Is<LookupPredicate>(p => p.CustomerId.Equals(CustomerId) && p.LegalEntityId.Equals(LegalEntityId) && p.EntityId.Equals(BankAccountId)));
        }

        private static bool LegalEntityIdCheck(MessageEnvelop envelop, string legalEntityId)
        {
            var bankAccount = envelop.Draft as BankAccount;

            return bankAccount.LegalEntityId.Equals(legalEntityId);
        }

        private static bool SameDraft(BankAccount bankAccount, MessageEnvelop messageEnvelop) 
        { 
            return bankAccount == messageEnvelop.Draft;
        }
    }
}

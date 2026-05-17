using Api.ApiModels;
using Api.Controllers;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Tests.Controllers
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
            customerDatabase.FindEntity<BankAccount>(Arg.Any<LookupPredicate>()).Returns(new MessageEnvelop() { CustomerId = CustomerId });
            bankAccountController = new BankAccountController(changeProcessor, customerDatabase, null);
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
            await bankAccountController.SubmitBankAccount(CustomerId, LegalEntityId, BankAccountId, new ApiContract.SubmitActionRequest() { Target_draft_version = 10 });

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
            var bankAccount = new ApiContract.CreateBankAccount();

            // Act
            await bankAccountController.CreateBankAccount(CustomerId, LegalEntityId, bankAccount);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<BankAccount>(Arg.Is<MessageEnvelop>(p => p.CustomerId.Equals(CustomerId) && LegalEntityIdCheck(p, LegalEntityId)));
        }

        [Fact]
        public async Task GetBankAccountById_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await bankAccountController.GetBankAccountById(CustomerId, LegalEntityId, BankAccountId);

            // Assert
            await customerDatabase.Received(1).FindEntity<BankAccount>(Arg.Is<LookupPredicate>(p => p.CustomerId.Equals(CustomerId) && p.LegalEntityId.Equals(LegalEntityId) && p.EntityId.Equals(BankAccountId)));
        }

        [Fact]
        public async Task UpdateBankAccount_WhenUpdating_ThenIssuesTheAppropriateCommand()
        {
            // Arrange
            var patchModel = new ApiContract.UpdateBankAccount()
            {
                Iban = "IBAN",
                Name = "BankName",
                Labels = ["Label"],
                Bank_country = "Country",
                Target_draft_version = 10
            };

            // Act
            await bankAccountController.UpdateBankAccount(CustomerId, LegalEntityId, BankAccountId, patchModel);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<BankAccount>(Arg.Is<MessageEnvelop>(m => SameAfterMapped(patchModel, m)));
        }

        private static bool SameAfterMapped(ApiContract.UpdateBankAccount bankAccount, MessageEnvelop messageEnvelop)
        {
            var bankAccountMapped = messageEnvelop.Draft as BankAccount;

            return messageEnvelop.Name == EntityName.BankAccount 
                && messageEnvelop.Change == ChangeType.Update
                && messageEnvelop.DraftVersion == 10
                && bankAccountMapped.Iban.Equals(bankAccount.Iban)
                && bankAccountMapped.BankName.Equals(bankAccountMapped.BankName)
                && bankAccountMapped.Labels.Equals(bankAccount.Labels)
                && bankAccountMapped.BankCountry.Equals(bankAccount.Bank_country);
        }

        private static bool LegalEntityIdCheck(MessageEnvelop envelop, string legalEntityId)
        {
            var bankAccount = envelop.Draft as BankAccount;

            return bankAccount.LegalEntityId.Equals(legalEntityId);
        }
    }
}

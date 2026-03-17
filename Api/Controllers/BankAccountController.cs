using Api.ApiModels;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/customers")]
    public class BankAccountController : EntityManagementController
    {
        public BankAccountController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase) : base(changeProcessor, customerDatabase)
        {
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}/touch")]
        public async Task<ActionResult<EntityDocumentModel>> TouchBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
        {
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Touch,
                Name = EntityName.BankAccount,
                CustomerId = customerId,
                EntityId = bankAccountId,
                Draft = new BankAccount()
                {
                    LegalEntityId = legalEntityId
                }
            };

            return await Touch(envelop);
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}/submit")]
        public async Task<ActionResult<EntityDocumentModel>> SubmitBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId, [FromBody] SubmitEntityModel submitModel)
        {
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Submit,
                Name = EntityName.BankAccount,
                CustomerId = customerId,
                EntityId = bankAccountId,
                IsSubmitted = true,
                Draft = new BankAccount()
                {
                    LegalEntityId = legalEntityId
                },
                DraftVersion = submitModel.TargetVersion
            };

            return await Submit<BankAccount>(envelop);
        }

        [HttpDelete("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<ActionResult<EntityDocumentModel>> RemoveBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
        {
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Delete,
                Name = EntityName.BankAccount,
                CustomerId = customerId,
                EntityId = bankAccountId,
                Draft = new BankAccount()
                {
                    LegalEntityId = legalEntityId
                },
            };

            return await Remove(envelop);
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/bank-accounts")]
        public async Task<ActionResult<EntityDocumentModel>> CreateBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromBody] BankAccount bankAccount)
        {
            bankAccount.LegalEntityId = legalEntityId; // Legal entity scoped.

            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.BankAccount,
                Draft = bankAccount,
                CustomerId = customerId
            };

            return await Create<BankAccount>(envelop);
        }

        [HttpGet("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<ActionResult<EntityDocumentModel>> GetBankAccountById(string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
        {
            return await GetById<BankAccount>(customerId, bankAccountId);
        }

        [HttpPatch("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<ActionResult<EntityDocumentModel>> UpateContact([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId, [FromBody] BankAccountModel patch)
        {
            var patchModel = ContactToPatch(patch);

            var envelop = new MessageEnvelop
            {
                EntityId = bankAccountId,
                Change = ChangeType.Update,
                Name = EntityName.BankAccount,
                Draft = patchModel,
                CustomerId = customerId,
                DraftVersion = patch.TargetVersion
            };

            await changeProcessor.ProcessChangeAsync(envelop);

            var contactEntity = await customerDatabase.GetEntity<BankAccount>(bankAccountId, customerId);

            return Translate(contactEntity);
        }

        private static BankAccount ContactToPatch(BankAccountModel patchModel)
        {
            var bankAccount = new BankAccount
            {
                Label = patchModel.Label,
                Name = patchModel.Name,
                AccountNumber = patchModel.AccountNumber,
                BankAccountHolderNames = patchModel.BankAccountHolderNames,
                BankCity = patchModel.BankCity,
                BankCountry = patchModel.BankCountry,
                BankName = patchModel.BankName,
                BillingDefault = patchModel.BillingDefault,
                Iban = patchModel.Iban,
                LegalEntityId = patchModel.LegalEntityId,
                Swift = patchModel.Swift,
                SortCode = patchModel.SortCode
            };

            if (patchModel.Descriptors != null)
            {
                bankAccount.Descriptors = [.. patchModel.Descriptors.Select(a => new Descriptor() { Key = a.Key, Value = a.Value })];
            }

            return bankAccount;
        }
    }
}

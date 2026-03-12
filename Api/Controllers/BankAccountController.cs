using Api.ApiModels;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
using StateManagment.Models;
using StateManagment.Services;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/customers")]
    public class BankAccountController : EntityManagementController
    {
        private readonly CustomerManagementService customerManager;

        public BankAccountController(CustomerManagementService customerManager) : base(customerManager)
        {
            this.customerManager = customerManager;
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
            return await Submit(EntityName.BankAccount, customerId, bankAccountId, submitModel);
        }

        [HttpDelete("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<ActionResult<EntityDocumentModel>> RemoveBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
        {
            return await Remove(EntityName.BankAccount, customerId, bankAccountId);
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/bank-accounts")]
        public async Task<ActionResult<EntityDocumentModel>> CreateBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromBody] BankAccount bankAccount)
        {
            return await Create(EntityName.BankAccount, customerId, bankAccount);
        }

        [HttpGet("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<ActionResult<EntityDocumentModel>> GetBankAccountById(string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
        {
            return await GetById(EntityName.BankAccount, customerId, bankAccountId);
        }

        [HttpPatch("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<ActionResult<EntityDocumentModel>> UpateContact([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId, [FromBody] BankAccountModel patch)
        {
            var patchModel = ContactToPatch(patch);
            await customerManager.Patch(patchModel, EntityName.BankAccount, customerId, bankAccountId, patch.TargetVersion, false);

            var contactEntity = await customerManager.Get(EntityName.BankAccount, customerId, bankAccountId);

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

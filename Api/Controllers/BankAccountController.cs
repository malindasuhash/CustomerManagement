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
        public async Task<StatusCodeResult> TouchBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
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
            var result = await SubmitForProcessing<BankAccount>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}/submit")]
        public async Task<ActionResult<ApiContract.SubmitActionResponse>> SubmitBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId, [FromBody] ApiContract.SubmitActionRequest submitActionRequest)
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
                DraftVersion = submitActionRequest.Draft_version
            };

            var result = await SubmitForProcessing<BankAccount>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return new ApiContract.SubmitActionResponse()
            {
                Entity_id = bankAccountId,
                Submitted_version = (long)result.SubmittedVersion
            };
        }

        [HttpDelete("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<StatusCodeResult> RemoveBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
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

            var result = await SubmitForProcessing<BankAccount>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/bank-accounts")]
        public async Task<ActionResult<ApiContract.EntityResponse_BankAccount>> CreateBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromBody] ApiContract.CreateUpdateBankAccount apiBankAccountRequest)
        {
            var bankAccount = ApiContractBankAccount_ToModelBankAccountMap.Convert(apiBankAccountRequest, legalEntityId);

            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.BankAccount,
                Draft = bankAccount,
                CustomerId = customerId
            };

            var result = await SubmitForProcessing<BankAccount>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return MessageEnvelop_ToEntityResponse_BankAccount.Convert(result);
        }

        [HttpGet("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_BankAccount>> GetBankAccountById(string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
        {
            var entityDocument = await customerDatabase.FindEntity<BankAccount>(LookupPredicate.Create(bankAccountId, customerId, legalEntityId));

            return MessageEnvelop_ToEntityResponse_BankAccount.Convert(entityDocument);
        }

        [HttpPatch("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<ActionResult<EntityDocumentModel>> UpdateBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId, [FromBody] BankAccountModel patch)
        {
            var patchModel = BankAccountToPatch(patch, legalEntityId);

            var envelop = new MessageEnvelop
            {
                EntityId = bankAccountId,
                Change = ChangeType.Update,
                Name = EntityName.BankAccount,
                Draft = patchModel,
                CustomerId = customerId,
                DraftVersion = patch.TargetVersion
            };

            return await Process<BankAccount>(envelop);
        }

        private static BankAccount BankAccountToPatch(BankAccountModel patchModel, string legalEntityId)
        {
            var bankAccount = new BankAccount
            {
                //Label = patchModel.Labels,
                Name = patchModel.Name,
                AccountNumber = patchModel.AccountNumber,
                BankAccountHolderNames = patchModel.BankAccountHolderNames,
                BankCity = patchModel.BankCity,
                BankCountry = patchModel.BankCountry,
                BankName = patchModel.BankName,
                BillingDefault = patchModel.BillingDefault,
                Iban = patchModel.Iban,
                LegalEntityId = legalEntityId,
                Swift = patchModel.Swift,
                SortCode = patchModel.SortCode
            };

            if (patchModel.MetaData != null)
            {
                //bankAccount.MetaDataModel = [.. patchModel.MetaDataModel.Select(a => new MetaDataModel() { Key = a.Key, Value = a.Value })];
            }

            return bankAccount;
        }
    }
}

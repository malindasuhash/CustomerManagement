using Api.Mappers;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<BankAccountController> _logger;

        public BankAccountController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase, ILogger<BankAccountController> logger) : base(changeProcessor, customerDatabase)
        {
            _logger = logger;
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}/touch")]
        public async Task<StatusCodeResult> TouchBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
        {
            _logger.LogInformation("TouchBankAccount called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, BankAccountId={BankAccountId}", customerId, legalEntityId, bankAccountId);

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
            _logger.LogInformation("SubmitBankAccount called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, BankAccountId={BankAccountId}, TargetDraftVersion={TargetDraft}", customerId, legalEntityId, bankAccountId, submitActionRequest?.Target_draft_version);
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
                DraftVersion = submitActionRequest.Target_draft_version
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
            _logger.LogInformation("RemoveBankAccount called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, BankAccountId={BankAccountId}", customerId, legalEntityId, bankAccountId);
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
        public async Task<ActionResult<ApiContract.EntityResponse_BankAccount>> CreateBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromBody] ApiContract.CreateBankAccount apiBankAccountRequest)
        {
            _logger.LogInformation("CreateBankAccount called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}", customerId, legalEntityId);
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

            return await GetBankAccountById(customerId, legalEntityId, result.EntityId);
        }

        [HttpGet("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_BankAccount>> GetBankAccountById(string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
        {
            _logger.LogInformation("GetBankAccountById called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, BankAccountId={BankAccountId}", customerId, legalEntityId, bankAccountId);
            var entityDocument = await customerDatabase.FindEntity<BankAccount>(LookupPredicate.Create(bankAccountId, customerId, legalEntityId));

            return MessageEnvelop_ToEntityResponse_BankAccount.Convert(entityDocument);
        }

        [HttpPatch("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_BankAccount>> UpdateBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId, [FromBody] ApiContract.UpdateBankAccount updateModelRequest)
        {
            _logger.LogInformation("UpdateBankAccount called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, BankAccountId={BankAccountId}, TargetDraftVersion={TargetDraft}", customerId, legalEntityId, bankAccountId, updateModelRequest?.Target_draft_version);
            var patchBankAccount = ApiContractBankAccount_ToModelBankAccountMap.Update(updateModelRequest, legalEntityId);

            var envelop = new MessageEnvelop
            {
                EntityId = bankAccountId,
                Change = ChangeType.Update,
                Name = EntityName.BankAccount,
                Draft = patchBankAccount,
                CustomerId = customerId,
                DraftVersion = (decimal)updateModelRequest.Target_draft_version
            };

            var result = await SubmitForProcessing<BankAccount>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return await GetBankAccountById(customerId, legalEntityId, bankAccountId);
        }
    }
}

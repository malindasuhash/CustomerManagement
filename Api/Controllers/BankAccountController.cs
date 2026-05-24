using Api.Mappers;
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
        private readonly ILogger<BankAccountController> logger;

        public BankAccountController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase, ILogger<BankAccountController> logger) : base(changeProcessor, customerDatabase, logger)
        {
            this.logger = logger;
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}/touch")]
        public async Task<StatusCodeResult> TouchBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
        {
            logger.LogInformation($"TouchBankAccount called for CustomerId={customerId}, LegalEntityId={legalEntityId}, BankAccountId={bankAccountId}");

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

            logger.LogInformation($"TouchBankAccount for CustomerId={customerId}, LegalEntityId={legalEntityId}, BankAccountId={bankAccountId}, completed successfully");

            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}/submit")]
        public async Task<ActionResult<ApiContract.SubmitActionResponse>> SubmitBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId, [FromBody] ApiContract.SubmitActionRequest submitActionRequest)
        {
            logger.LogInformation("SubmitBankAccount called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, BankAccountId={BankAccountId}, TargetDraftVersion={TargetDraft}", customerId, legalEntityId, bankAccountId, submitActionRequest?.Target_draft_version);
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

            logger.LogInformation($"SubmitBankAccount for CustomerId={customerId}, LegalEntityId={legalEntityId}, BankAccountId={bankAccountId}, completed successfully");

            return new ApiContract.SubmitActionResponse()
            {
                Entity_id = bankAccountId,
                Submitted_version = (long)result.SubmittedVersion
            };
        }

        [HttpDelete("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<StatusCodeResult> RemoveBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
        {
            logger.LogInformation($"RemoveBankAccount called for CustomerId={customerId}, LegalEntityId={legalEntityId}, BankAccountId={bankAccountId}");

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

            logger.LogInformation($"RemoveBankAccount for CustomerId={customerId}, LegalEntityId={legalEntityId}, BankAccountId={bankAccountId} completed successfully");

            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/bank-accounts")]
        public async Task<ActionResult<ApiContract.EntityResponse_BankAccount>> CreateBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromBody] ApiContract.CreateBankAccount apiBankAccountRequest)
        {
            logger.LogInformation($"CreateBankAccount called for CustomerId={customerId}, LegalEntityId={legalEntityId}");

            var bankAccount = ApiContractBankAccount_ToModelBankAccountMap.Convert(apiBankAccountRequest, legalEntityId);

            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.BankAccount,
                Draft = bankAccount,
                CustomerId = customerId
            };

            logger.LogInformation($"Submitting to create BankAccount with CustomerId={customerId}, LegalEntityId={legalEntityId}");

            var result = await SubmitForProcessing<BankAccount>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"CreateBankAccount for CustomerId={customerId}, LegalEntityId={legalEntityId} completed successfully. BankAccountId='{result.EntityId}'");

            return await GetBankAccountById(customerId, legalEntityId, result.EntityId);
        }

        [HttpGet("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_BankAccount>> GetBankAccountById(string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId)
        {
            logger.LogInformation($"GetBankAccountById called for CustomerId={customerId}, LegalEntityId={legalEntityId}, BankAccountId={bankAccountId}");
            var entityDocument = await customerDatabase.FindEntity<BankAccount>(LookupPredicate.Create(bankAccountId, customerId, legalEntityId));

            return MessageEnvelop_ToEntityResponse_BankAccount.Convert(entityDocument);
        }

        [HttpPatch("{customerId}/legal-entities/{legalEntityId}/bank-accounts/{bankAccountId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_BankAccount>> UpdateBankAccount([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string bankAccountId, [FromBody] ApiContract.UpdateBankAccount updateModelRequest)
        {
            logger.LogInformation($"UpdateBankAccount called for CustomerId={customerId}, LegalEntityId={legalEntityId}, BankAccountId={bankAccountId}, TargetDraftVersion={updateModelRequest.Target_draft_version}");

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

            logger.LogInformation($"UpdateBankAccount for CustomerId={customerId}, LegalEntityId={legalEntityId}, BankAccountId={bankAccountId}, TargetDraftVersion={updateModelRequest.Target_draft_version} completed successfully");

            return await GetBankAccountById(customerId, legalEntityId, bankAccountId);
        }
    }
}

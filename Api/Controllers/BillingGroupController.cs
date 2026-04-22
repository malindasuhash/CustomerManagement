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
    public class BillingGroupController : EntityManagementController
    {
        public BillingGroupController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase) : base(changeProcessor, customerDatabase)
        {
        }

        [HttpPost("{customerId}/billing-groups/{billingGroupId}/touch")]
        public async Task<ActionResult<EntityDocumentModel>> TouchBillingGroup([FromRoute] string customerId, [FromRoute] string billingGroupId)
        {
            var envelop = new MessageEnvelop()
            {
                CustomerId = customerId,
                Change = ChangeType.Touch,
                EntityId = billingGroupId,
                Name = EntityName.BillingGroup
            };

            return await Process<BillingGroup>(envelop);
        }

        [HttpPost("{customerId}/billing-groups/{billingGroupId}/submit")]
        public async Task<ActionResult<EntityDocumentModel>> SubmitBillingGroup([FromRoute] string customerId, [FromRoute] string billingGroupId, [FromBody] SubmitEntityModel submitModel)
        {
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Submit,
                Name = EntityName.BillingGroup,
                CustomerId = customerId,
                EntityId = billingGroupId,
                IsSubmitted = true,
                DraftVersion = submitModel.TargetVersion
            };

            return await Process<BillingGroup>(envelop);
        }

        [HttpDelete("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<ActionResult<EntityDocumentModel>> RemoveBillingGroup([FromRoute] string customerId, [FromRoute] string billingGroupId)
        {
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Delete,
                Name = EntityName.BillingGroup,
                CustomerId = customerId,
                EntityId = billingGroupId,
            };

            return await Process<BillingGroup>(envelop);
        }

        [HttpPost("{customerId}/billing-groups")]
        public async Task<ActionResult<EntityDocumentModel>> CreateBillingGroup([FromRoute] string customerId, [FromBody] ApiContract.CreateBillingGroup billingGroup)
        {
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.BillingGroup,
                Draft = NewBillingGroup(billingGroup),
                CustomerId = customerId
            };

            return await Process<BillingGroup>(envelop);
        }

        [HttpGet("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<ActionResult<EntityDocumentModel>> GetBillingGroupById(string customerId, string billingGroupId)
        {
            return await GetById<BillingGroup>(LookupPredicate.Create(billingGroupId, customerId));
        }

        [HttpPatch("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<ActionResult<EntityDocumentModel>> UpateBillingGroup([FromRoute] string customerId, [FromRoute] string billingGroupId, [FromBody] BillingGroupModel patch)
        {
            var patchModel = BillingGroupToPatch(patch);

            var envelop = new MessageEnvelop
            {
                EntityId = billingGroupId,
                Change = ChangeType.Update,
                Name = EntityName.BillingGroup,
                Draft = patchModel,
                CustomerId = customerId,
                DraftVersion = patch.TargetVersion
            };

            return await Process<BillingGroup>(envelop);
        }

        private static BillingGroup BillingGroupToPatch(BillingGroupModel patchModel)
        {
            var billingGroup = new BillingGroup
            {
                BillingBankAccountId = patchModel.BillingBankAccountId,
                // Label = patchModel.Label,
                Name = patchModel.Name,
                LegalEntityId = patchModel.LegalEntityId,
                Description = patchModel.Description
            };

            if (patchModel.Descriptors != null)
            {
                billingGroup.Descriptors = [.. patchModel.Descriptors.Select(a => new Descriptor() { Key = a.Key, Value = a.Value })];
            }

            return billingGroup;
        }

        private BillingGroup NewBillingGroup(ApiContract.CreateBillingGroup billingGroup)
        {
            return new BillingGroup
            {
                BillingBankAccountId = billingGroup.Billing_bank_account_id,
                Labels = billingGroup.Labels.ToArray(),
                Name = billingGroup.Name,
                LegalEntityId = billingGroup.Legal_entity_id,
                Description = billingGroup.Description
            };
        }

        private ApiContract.BillingGroup BillingGroupModelToContract(BillingGroupModel billingGroupModel)
        {
            return new ApiContract.BillingGroup
            {
                Billing_bank_account_id = billingGroupModel.BillingBankAccountId,
                Labels = billingGroupModel.Labels,
                Name = billingGroupModel.Name,
                Legal_entity_id = billingGroupModel.LegalEntityId,
                Description = billingGroupModel.Description
            };
        }
    }
}

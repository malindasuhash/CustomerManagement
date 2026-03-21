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
        public async Task<ActionResult<EntityDocumentModel>> TouchContact([FromRoute] string customerId, [FromRoute] string billingGroupId)
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
        public async Task<ActionResult<EntityDocumentModel>> SubmitContact([FromRoute] string customerId, [FromRoute] string billingGroupId, [FromBody] SubmitEntityModel submitModel)
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
        public async Task<ActionResult<EntityDocumentModel>> RemoveContact([FromRoute] string customerId, [FromRoute] string billingGroupId)
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
        public async Task<ActionResult<EntityDocumentModel>> CreateContact([FromRoute] string customerId, [FromBody] BillingGroup billingGroup)
        {
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.BankAccount,
                Draft = billingGroup,
                CustomerId = customerId
            };

            return await Process<Contact>(envelop);
        }

        [HttpGet("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<ActionResult<EntityDocumentModel>> GetContactById(string customerId, string billingGroupId)
        {
            return await GetById<BillingGroup>(LookupPredicate.Create(billingGroupId, customerId));
        }

        [HttpPatch("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<ActionResult<EntityDocumentModel>> UpateContact([FromRoute] string customerId, [FromRoute] string billingGroupId, [FromBody] BillingGroupModel patch)
        {
            var patchModel = ContactToPatch(patch);
            
            var envelop = new MessageEnvelop
            {
                EntityId = billingGroupId,
                Change = ChangeType.Update,
                Name = EntityName.BillingGroup,
                Draft = patchModel,
                CustomerId = customerId,
                DraftVersion = patch.TargetVersion
            };

            await changeProcessor.ProcessChangeAsync<BillingGroup>(envelop);

            var billingGroup = await customerDatabase.FindEntity<BillingGroup>(envelop.SearchBy());

            return Translate(billingGroup);
        }

        private static BillingGroup ContactToPatch(BillingGroupModel patchModel)
        {
            var billingGroup = new BillingGroup
            {
                BillingBankAccountId = patchModel.BillingBankAccountId,
                Label = patchModel.Label,
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
    }
}

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
    public class BillingGroupController : EntityManagementController
    {
        private readonly CustomerManagementService customerManager;

        public BillingGroupController(CustomerManagementService customerManager) : base(customerManager)
        {
            this.customerManager = customerManager;
        }

        [HttpPost("{customerId}/billing-groups/{billingGroupId}/touch")]
        public async Task<ActionResult<EntityDocumentModel>> TouchContact([FromRoute] string customerId, [FromRoute] string billingGroupId)
        {
            return await Touch(EntityName.BillingGroup, customerId, billingGroupId);
        }

        [HttpPost("{customerId}/billing-groups/{billingGroupId}/submit")]
        public async Task<ActionResult<EntityDocumentModel>> SubmitContact([FromRoute] string customerId, [FromRoute] string billingGroupId, [FromBody] SubmitEntityModel submitModel)
        {
            return await Submit(EntityName.BillingGroup, customerId, billingGroupId, submitModel);
        }

        [HttpDelete("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<ActionResult<EntityDocumentModel>> RemoveContact([FromRoute] string customerId, [FromRoute] string billingGroupId)
        {
            return await Remove(EntityName.BillingGroup, customerId, billingGroupId);
        }

        [HttpPost("{customerId}/billing-groups")]
        public async Task<ActionResult<EntityDocumentModel>> CreateContact([FromRoute] string customerId, [FromBody] BillingGroup billingGroup)
        {
            return await Create(EntityName.BillingGroup, customerId, billingGroup);
        }

        [HttpGet("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<ActionResult<EntityDocumentModel>> GetContactById(string customerId, string billingGroupId)
        {
            return await GetById(EntityName.BillingGroup, customerId, billingGroupId);
        }

        [HttpPatch("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<ActionResult<EntityDocumentModel>> UpateContact([FromRoute] string customerId, [FromRoute] string billingGroupId, [FromBody] BillingGroupModel patch)
        {
            var patchModel = ContactToPatch(patch);
            await customerManager.Patch(patchModel, EntityName.BillingGroup, customerId, billingGroupId, patch.TargetVersion, false);

            var contactEntity = await customerManager.Get(EntityName.BillingGroup, customerId, billingGroupId);

            return Translate(contactEntity);
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

using Api.ApiModels;
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
    public class BillingGroupController : EntityManagementController
    {
        public BillingGroupController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase) : base(changeProcessor, customerDatabase)
        {
        }

        [HttpPost("{customerId}/billing-groups/{billingGroupId}/touch")]
        public async Task<ActionResult<StatusCodeResult>> TouchBillingGroup([FromRoute] string customerId, [FromRoute] string billingGroupId)
        {
            var envelop = new MessageEnvelop()
            {
                CustomerId = customerId,
                Change = ChangeType.Touch,
                EntityId = billingGroupId,
                Name = EntityName.BillingGroup
            };

            var result = await SubmitForProcessing<BillingGroup>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return new NoContentResult();
        }

        [HttpPost("{customerId}/billing-groups/{billingGroupId}/submit")]
        public async Task<ActionResult<ApiContract.SubmitActionResponse>> SubmitBillingGroup([FromRoute] string customerId, [FromRoute] string billingGroupId, [FromBody] ApiContract.SubmitActionRequest submitActionRequest)
        {
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Submit,
                Name = EntityName.BillingGroup,
                CustomerId = customerId,
                EntityId = billingGroupId,
                IsSubmitted = true,
                DraftVersion = submitActionRequest.Target_draft_version
            };

            var result = await SubmitForProcessing<BillingGroup>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return new ApiContract.SubmitActionResponse()
            {
                Entity_id = billingGroupId,
                Submitted_version = (long)result.SubmittedVersion
            };
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

            var result = await SubmitForProcessing<BillingGroup>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return new NoContentResult();
        }

        [HttpPost("{customerId}/billing-groups")]
        public async Task<ActionResult<ApiContract.EntityResponse_BillingGroup>> CreateBillingGroup([FromRoute] string customerId, [FromBody] ApiContract.CreateBillingGroup billingGroup)
        {
            var domainBillingGroup = ApiContractBillingGroup_ToModelBillingGroupMap.Convert(billingGroup);

            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.BillingGroup,
                Draft = domainBillingGroup,
                CustomerId = customerId
            };

            var result = await SubmitForProcessing<BillingGroup>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return MessageEnvelop_ToEntityResponse_BillingGroup.Convert(result);
        }

        [HttpGet("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_BillingGroup>> GetBillingGroupById(string customerId, string billingGroupId)
        {
            var entityDocument = await customerDatabase.FindEntity<BillingGroup>(LookupPredicate.Create(billingGroupId, customerId));
            if (entityDocument == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return MessageEnvelop_ToEntityResponse_BillingGroup.Convert(entityDocument);
        }

        [HttpPatch("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_BillingGroup>> UpateBillingGroup([FromRoute] string customerId, [FromRoute] string billingGroupId, [FromBody] ApiContract.BillingGroup patch)
        {
            var patchModel = ApiContractBillingGroup_ToModelBillingGroupMap.Update(patch);

            var envelop = new MessageEnvelop
            {
                EntityId = billingGroupId,
                Change = ChangeType.Update,
                Name = EntityName.BillingGroup,
                Draft = patchModel,
                CustomerId = customerId,
                DraftVersion = 0 // TODO: Define a new property in ApiContract.UpdateBillingGroup
            };

            var result = await SubmitForProcessing<BillingGroup>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return await GetBillingGroupById(customerId, billingGroupId);
        }
    }
}

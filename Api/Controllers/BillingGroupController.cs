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
        private readonly ILogger<BillingGroupController> logger;

        public BillingGroupController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase, ILogger<BillingGroupController> logger) : base(changeProcessor, customerDatabase, logger)
        {
            this.logger = logger;
        }

        [HttpPost("{customerId}/billing-groups/{billingGroupId}/touch")]
        public async Task<StatusCodeResult> TouchBillingGroup([FromRoute] string customerId, [FromRoute] string billingGroupId)
        {
            logger.LogInformation("Touching billing group with id {BillingGroupId} for customer {CustomerId}", billingGroupId, customerId);

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

            logger.LogInformation("Successfully touched billing group with id {BillingGroupId} for customer {CustomerId}", billingGroupId, customerId);

            return new NoContentResult();
        }

        [HttpPost("{customerId}/billing-groups/{billingGroupId}/submit")]
        public async Task<ActionResult<ApiContract.SubmitActionResponse>> SubmitBillingGroup([FromRoute] string customerId, [FromRoute] string billingGroupId, [FromBody] ApiContract.SubmitActionRequest submitActionRequest)
        {
            logger.LogInformation("Submitting billing group with id {BillingGroupId} for customer {CustomerId}", billingGroupId, customerId);
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

            logger.LogInformation("Successfully submitted billing group with id {BillingGroupId} for customer {CustomerId}", billingGroupId, customerId);

            return new ApiContract.SubmitActionResponse()
            {
                Entity_id = billingGroupId,
                Submitted_version = (long)result.SubmittedVersion
            };
        }

        [HttpDelete("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<StatusCodeResult> RemoveBillingGroup([FromRoute] string customerId, [FromRoute] string billingGroupId)
        {
            logger.LogInformation("Removing billing group with id {BillingGroupId} for customer {CustomerId}", billingGroupId, customerId);
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

            logger.LogInformation("Successfully removed billing group with id {BillingGroupId} for customer {CustomerId}", billingGroupId, customerId);

            return new NoContentResult();
        }

        [HttpPost("{customerId}/billing-groups")]
        public async Task<ActionResult<ApiContract.EntityResponse_BillingGroup>> CreateBillingGroup([FromRoute] string customerId, [FromBody] ApiContract.CreateBillingGroup billingGroup)
        {
            logger.LogInformation("Creating billing group for customer {CustomerId}", customerId);
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

            logger.LogInformation("Successfully created billing group with id {BillingGroupId} for customer {CustomerId}", result.EntityId, customerId);

            return MessageEnvelop_ToEntityResponse_BillingGroup.Convert(result);
        }

        [HttpGet("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_BillingGroup>> GetBillingGroupById(string customerId, string billingGroupId)
        {
            logger.LogInformation("Getting billing group with id {BillingGroupId} for customer {CustomerId}", billingGroupId, customerId);

            var entityDocument = await customerDatabase.FindEntity<BillingGroup>(LookupPredicate.Create(billingGroupId, customerId));
            if (entityDocument == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("Successfully got billing group with id {BillingGroupId} for customer {CustomerId}", billingGroupId, customerId);

            return MessageEnvelop_ToEntityResponse_BillingGroup.Convert(entityDocument);
        }

        [HttpPatch("{customerId}/billing-groups/{billingGroupId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_BillingGroup>> UpateBillingGroup([FromRoute] string customerId, [FromRoute] string billingGroupId, [FromBody] ApiContract.UpdateBillingGroup patch)
        {
            logger.LogInformation("Updating billing group with id {BillingGroupId} for customer {CustomerId}", billingGroupId, customerId);

            var patchModel = ApiContractBillingGroup_ToModelBillingGroupMap.Update(patch);

            var envelop = new MessageEnvelop
            {
                EntityId = billingGroupId,
                Change = ChangeType.Update,
                Name = EntityName.BillingGroup,
                Draft = patchModel,
                CustomerId = customerId,
                DraftVersion = (decimal) patch.Target_draft_version
            };

            var result = await SubmitForProcessing<BillingGroup>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("Successfully updated billing group with id {BillingGroupId} for customer {CustomerId} and {TargetVersion}", billingGroupId, customerId, patch.Target_draft_version);

            return await GetBillingGroupById(customerId, billingGroupId);
        }
    }
}

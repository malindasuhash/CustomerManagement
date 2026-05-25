using Api.ApiModels;
using Api.Mappers;
using Api.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/customers")]
    public class LegalEntityController : EntityManagementController
    {
        private readonly LinkGenerator linkGenerator;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<LegalEntityController> logger;

        public LegalEntityController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase, LinkGenerator linkGenerator, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<LegalEntityController> logger) : base(changeProcessor, customerDatabase, logger)
        {
            this.linkGenerator = linkGenerator;
            this.httpClientFactory = httpClientFactory;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        [HttpGet("{customerId}/legal-entities/{entityId}/changes")]
        public async Task<ChangeSummary> GetChanges([FromRoute] string customerId, [FromQuery] string entityId)
        {
            logger.LogInformation($"Getting changes for legal entity {entityId} of customer {customerId}");

            var pendingChanges = await GetLinks(customerId, entityId);

            logger.LogInformation($"Found {pendingChanges.Length} pending changes for legal entity {entityId} of customer {customerId}");

            return new ChangeSummary()
            {
                total = pendingChanges.Length,
                Changes = pendingChanges
            };
        }

        [HttpPost("{customerId}/legal-entities/{entityId}/changes")]
        public async Task<ChangeSummarySubmitResult> ChangeSubmitResults([FromRoute] string customerId, [FromQuery] string entityId)
        {
            logger.LogInformation($"Submitting changes for legal entity {entityId} of customer {customerId}");

            var pendingChanges = await GetLinks(customerId, entityId);
            var changeSubmitter = new ChangeSubmitter(httpClientFactory, httpContextAccessor);

            var submitResults = await changeSubmitter.SubmitAll(pendingChanges);

            logger.LogInformation($"Submitted {submitResults.Count} changes for legal entity {entityId} of customer {customerId} with {submitResults.Count(r => r.Result.Equals(ChangeSubmitter.SubmitAction))} successes and {submitResults.Count(r => !r.Result.Equals(ChangeSubmitter.FailedAction))} failures");

            return new ChangeSummarySubmitResult()
            {
                total = submitResults.Count,
                Changes = submitResults
            };
        }

        [HttpPost("{customerId}/legal-entities/{entityId}/touch")]
        public async Task<StatusCodeResult> TouchLegalEntity([FromRoute] string customerId, [FromRoute] string entityId)
        {
            logger.LogInformation($"Touching legal entity {entityId} of customer {customerId}");

            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Touch,
                Name = EntityName.LegalEntity,
                EntityId = entityId,
                CustomerId = customerId
            };

            var result = await SubmitForProcessing<LegalEntity>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"Touched legal entity {entityId} of customer {customerId}");

            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities/{entityId}/submit")]
        public async Task<ActionResult<ApiContract.SubmitActionResponse>> SubmitLegalEntity([FromRoute] string customerId, [FromRoute] string entityId, [FromBody] ApiContract.SubmitActionRequest submitActionRequest)
        {
            logger.LogInformation($"Submitting legal entity {entityId} of customer {customerId} with target draft version {submitActionRequest.Target_draft_version}");
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Submit,
                Name = EntityName.LegalEntity,
                CustomerId = customerId,
                EntityId = entityId,
                IsSubmitted = true,
                DraftVersion = submitActionRequest.Target_draft_version
            };

            var result = await SubmitForProcessing<LegalEntity>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"Submitted legal entity {entityId} of customer {customerId} with submitted version {result.SubmittedVersion}");
            return new ApiContract.SubmitActionResponse()
            {
                Entity_id = result.EntityId,
                Submitted_version = (long)result.SubmittedVersion
            };
        }

        [HttpDelete("{customerId}/legal-entities/{entityId}")]
        public async Task<StatusCodeResult> RemoveLegalEntity([FromRoute] string customerId, [FromRoute] string entityId)
        {
            logger.LogInformation($"Removing legal entity {entityId} of customer {customerId}");
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Delete,
                Name = EntityName.LegalEntity,
                CustomerId = customerId,
                EntityId = entityId
            };

            var result = await SubmitForProcessing<LegalEntity>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"Removed legal entity {entityId} of customer {customerId}");

            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities")]
        public async Task<ActionResult<ApiContract.EntityResponse_LegalEntity>> CreateLegalEntity([FromRoute] string customerId, [FromBody] ApiContract.CreateLegalEntity legalEntity)
        {
            logger.LogInformation($"Creating legal entity for customer {customerId} with name {legalEntity.Name}");

            var domainLegalEntity = ApiContractLegalEntity_ToModelLegalEntityMap.Convert(legalEntity);

            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.LegalEntity,
                Draft = domainLegalEntity,
                CustomerId = customerId
            };

            var result = await SubmitForProcessing<LegalEntity>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"Created legal entity {result.EntityId} for customer {customerId} with name {legalEntity.Name}");
            return MessageEnvelop_ToEntityResponseLegalEntityMap.Convert(result);
        }

        [HttpGet("{customerId}/legal-entities/{entityId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_LegalEntity>> GetLegalEntityById(string customerId, string entityId)
        {
            logger.LogInformation($"Getting legal entity {entityId} for customer {customerId}");

            var entityDocument = await customerDatabase.FindEntity<LegalEntity>(LookupPredicate.Create(entityId, customerId));
            if (entityDocument == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"Found legal entity {entityId} for customer {customerId}");
            return MessageEnvelop_ToEntityResponseLegalEntityMap.Convert(entityDocument);
        }

        [HttpPatch("{customerId}/legal-entities/{entityId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_LegalEntity>> UpdateLegalEntity([FromRoute] string customerId, [FromRoute] string entityId, [FromBody] ApiContract.UpdateLegalEntity patch)
        {
            logger.LogInformation($"Updating legal entity {entityId} for customer {customerId} with target draft version {patch.Target_draft_version}");

            LegalEntity patchModel = MessageEnvelop_ToEntityResponseLegalEntityMap.Convert(patch);

            var envelop = new MessageEnvelop
            {
                EntityId = entityId,
                Change = ChangeType.Update,
                Name = EntityName.LegalEntity,
                Draft = patchModel,
                CustomerId = customerId,
                DraftVersion = (decimal)patch.Target_draft_version
            };

            var result = await SubmitForProcessing<LegalEntity>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"Updated legal entity {entityId} for customer {customerId} with target draft version {patch.Target_draft_version}");
            return await GetLegalEntityById(customerId, entityId);
        }

        private async Task<ChangeLink[]> GetLinks(string customerId, string? legalEntityId)
        {
            var pendingChanges = await customerDatabase.GetPendingChanges(customerId, legalEntityId);
            return pendingChanges.Select(change => ChangeLink.Create(change, linkGenerator, customerId, legalEntityId)).ToArray();
        }
    }
}

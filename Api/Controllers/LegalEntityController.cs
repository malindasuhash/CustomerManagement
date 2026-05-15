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

        public LegalEntityController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase, LinkGenerator linkGenerator, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : base(changeProcessor, customerDatabase)
        {
            this.linkGenerator = linkGenerator;
            this.httpClientFactory = httpClientFactory;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("{customerId}/legal-entities/{entityId}/changes")]
        public async Task<ChangeSummary> GetChanges([FromRoute] string customerId, [FromQuery] string entityId)
        {
            var pendingChanges = await GetLinks(customerId, entityId);

            return new ChangeSummary()
            {
                total = pendingChanges.Length,
                Changes = pendingChanges
            };
        }

        [HttpPost("{customerId}/legal-entities/{entityId}/changes")]
        public async Task<ChangeSummarySubmitResult> ChangeSubmitResults([FromRoute] string customerId, [FromQuery] string entityId)
        {
            var pendingChanges = await GetLinks(customerId, entityId);
            var changeSubmitter = new ChangeSubmitter(httpClientFactory, httpContextAccessor);

            var submitResults = await changeSubmitter.SubmitAll(pendingChanges);

            return new ChangeSummarySubmitResult()
            {
                total = submitResults.Count,
                Changes = submitResults
            };
        }

        [HttpPost("{customerId}/legal-entities/{entityId}/touch")]
        public async Task<StatusCodeResult> TouchLegalEntity([FromRoute] string customerId, [FromRoute] string entityId)
        {
            // LEGAL_ENTITY_TOUCH
            // LEGAL_ENTITY_READ
            // SYSTEM_DATA_READ
            // SOFTDELETE_DATA_READ
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

            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities/{entityId}/submit")]
        public async Task<ActionResult<ApiContract.SubmitActionResponse>> SubmitLegalEntity([FromRoute] string customerId, [FromRoute] string entityId, [FromBody] ApiContract.SubmitActionRequest submitActionRequest)
        {
            // LEGAL_ENTITY_SUBMIT
            // LEGAL_ENTITY_READ
            // SYSTEM_DATA_READ
            // SOFTDELETE_DATA_READ
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

            return new ApiContract.SubmitActionResponse()
            {
                Entity_id = result.EntityId,
                Submitted_version = (long)result.SubmittedVersion
            };
        }

        [HttpDelete("{customerId}/legal-entities/{entityId}")]
        public async Task<StatusCodeResult> RemoveLegalEntity([FromRoute] string customerId, [FromRoute] string entityId)
        {
            // LEGAL_ENTITY_REMOVE
            // LEGAL_ENTITY_READ
            // SYSTEM_DATA_READ
            // SOFTDELETE_DATA_READ
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

            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities")]
        public async Task<ActionResult<ApiContract.EntityResponse_LegalEntity>> CreateLegalEntity([FromRoute] string customerId, [FromBody] ApiContract.CreateLegalEntity legalEntity)
        {
            // LEGAL_ENTITY_WRITE
            // LEGAL_ENTITY_READ
            // SYSTEM_DATA_READ
            // SOFTDELETE_DATA_READ

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

            return MessageEnvelop_ToEntityResponseLegalEntityMap.Convert(result);
        }

        [HttpGet("{customerId}/legal-entities/{entityId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_LegalEntity>> GetLegalEntityById(string customerId, string entityId)
        {
            // LEGAL_ENTITY_READ
            // SYSTEM_DATA_READ
            // SOFTDELETE_DATA_READ
            var entityDocument = await customerDatabase.FindEntity<LegalEntity>(LookupPredicate.Create(entityId, customerId));
            if (entityDocument == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return MessageEnvelop_ToEntityResponseLegalEntityMap.Convert(entityDocument);
        }

        [HttpPatch("{customerId}/legal-entities/{entityId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_LegalEntity>> UpdateLegalEntity([FromRoute] string customerId, [FromRoute] string entityId, [FromBody] ApiContract.UpdateLegalEntity patch)
        {
            // LEGAL_ENTITY_UPDATE
            // LEGAL_ENTITY_READ
            // SYSTEM_DATA_READ
            // SOFTDELETE_DATA_READ
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

            return await GetLegalEntityById(customerId, entityId);
        }

        private async Task<ChangeLink[]> GetLinks(string customerId, string? legalEntityId)
        {
            var pendingChanges = await customerDatabase.GetPendingChanges(customerId, legalEntityId);
            return pendingChanges.Select(change => ChangeLink.Create(change, linkGenerator, customerId, legalEntityId)).ToArray();
        }
    }
}

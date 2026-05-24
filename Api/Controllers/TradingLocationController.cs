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
    public class TradingLocationController : EntityManagementController
    {
        private readonly ILogger<TradingLocationController> logger;

        public TradingLocationController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase, ILogger<TradingLocationController> logger) : base(changeProcessor, customerDatabase, logger)
        {
            this.logger = logger;
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/trading-locations")]
        public async Task<ActionResult<ApiContract.EntityResponse_TradingLocation>> CreateTradingLocation(string customerId, string legalEntityId, [FromBody] ApiContract.CreateTradingLocation tradingLocation)
        {
            logger.LogInformation("CreateTradingLocation called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}", customerId, legalEntityId);

            var domainTradingLocation = ApiContractTradingLocation_ToModelTradingLocationMap.Convert(tradingLocation, legalEntityId);

            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.TradingLocation,
                Draft = domainTradingLocation,
                CustomerId = customerId
            };

            var result = await SubmitForProcessing<TradingLocation>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("CreateTradingLocation processed for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, TradingLocationId={TradingLocationId}", customerId, legalEntityId, result.EntityId);

            return await GetTradingLocationById(customerId, legalEntityId, result.EntityId);
        }

        [HttpGet("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_TradingLocation>> GetTradingLocationById([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId)
        {
            logger.LogInformation("GetTradingLocationById called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, TradingLocationId={TradingLocationId}", customerId, legalEntityId, tradingLocationId);

            var entityDocument = await customerDatabase.FindEntity<TradingLocation>(LookupPredicate.Create(tradingLocationId, customerId, legalEntityId));
            if (entityDocument == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("GetTradingLocationById found entity for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, TradingLocationId={TradingLocationId}", customerId, legalEntityId, tradingLocationId);

            return MessageEnvelop_ToEntityResponse_TradingLocation.Convert(entityDocument);
        }

        [HttpDelete("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}")]
        public async Task<StatusCodeResult> RemoveTradingLocation([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId)
        {
            logger.LogInformation("RemoveTradingLocation called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, TradingLocationId={TradingLocationId}", customerId, legalEntityId, tradingLocationId);

            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Delete,
                Name = EntityName.TradingLocation,
                CustomerId = customerId,
                EntityId = tradingLocationId,
                Draft = new TradingLocation()
                {
                    LegalEntityId = legalEntityId
                },
            };

            var result = await SubmitForProcessing<TradingLocation>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("RemoveTradingLocation processed for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, TradingLocationId={TradingLocationId}", customerId, legalEntityId, tradingLocationId);

            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}/submit")]
        public async Task<ActionResult<ApiContract.SubmitActionResponse>> SubmitTradingLocation([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId, [FromBody] ApiContract.SubmitActionRequest submitActionRequest)
        {
            logger.LogInformation("SubmitTradingLocation called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, TradingLocationId={TradingLocationId}, TargetDraftVersion={TargetDraftVersion}", customerId, legalEntityId, tradingLocationId, submitActionRequest?.Target_draft_version);
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Submit,
                Name = EntityName.TradingLocation,
                CustomerId = customerId,
                EntityId = tradingLocationId,
                IsSubmitted = true,
                Draft = new TradingLocation()
                {
                    LegalEntityId = legalEntityId
                },
                DraftVersion = submitActionRequest.Target_draft_version
            };

            var result = await SubmitForProcessing<TradingLocation>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("SubmitTradingLocation processed for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, TradingLocationId={TradingLocationId}, SubmittedVersion={SubmittedVersion}", customerId, legalEntityId, tradingLocationId, result.SubmittedVersion);

            return new ApiContract.SubmitActionResponse()
            {
                Entity_id = legalEntityId,
                Submitted_version = (long)result.SubmittedVersion
            };
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}/touch")]
        public async Task<StatusCodeResult> TouchTradingLocation([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId)
        {
            logger.LogInformation("TouchTradingLocation called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, TradingLocationId={TradingLocationId}", customerId, legalEntityId, tradingLocationId);

            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Touch,
                Name = EntityName.TradingLocation,
                CustomerId = customerId,
                EntityId = tradingLocationId,
                Draft = new TradingLocation()
                {
                    LegalEntityId = legalEntityId
                }
            };

            var result = await SubmitForProcessing<TradingLocation>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("TouchTradingLocation processed for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, TradingLocationId={TradingLocationId}", customerId, legalEntityId, tradingLocationId);

            return new NoContentResult();
        }

        [HttpPatch("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_TradingLocation>> UpdateTradingLocation([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId, ApiContract.UpdateTradingLocation updateTradingLocation)
        {
            logger.LogInformation("UpdateTradingLocation called for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, TradingLocationId={TradingLocationId}, TargetDraftVersion={TargetDraftVersion}", customerId, legalEntityId, tradingLocationId, updateTradingLocation?.Target_draft_version);

            var patch = ApiContractTradingLocation_ToModelTradingLocationMap.Update(updateTradingLocation, legalEntityId);

            var envelop = new MessageEnvelop
            {
                EntityId = tradingLocationId,
                Change = ChangeType.Update,
                Name = EntityName.TradingLocation,
                Draft = patch,
                CustomerId = customerId,
                DraftVersion = (decimal)updateTradingLocation.Target_draft_version
            };

            var result = await SubmitForProcessing<TradingLocation>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("UpdateTradingLocation processed for CustomerId={CustomerId}, LegalEntityId={LegalEntityId}, TradingLocationId={TradingLocationId}, DraftVersion={DraftVersion}", customerId, legalEntityId, tradingLocationId, result.DraftVersion);

            return await GetTradingLocationById(customerId, legalEntityId, tradingLocationId);
        }
    }
}

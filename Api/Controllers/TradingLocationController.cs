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
        public TradingLocationController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase) : base(changeProcessor, customerDatabase)
        {
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/trading-locations")]
        public async Task<ActionResult<ApiContract.EntityResponse_TradingLocation>> CreateTradingLocation(string customerId, string legalEntityId, [FromBody] ApiContract.CreateTradingLocation tradingLocation)
        {
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

            return await GetTradingLocationById(customerId, legalEntityId, result.EntityId);
        }

        [HttpGet("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_TradingLocation>> GetTradingLocationById([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId)
        {
            var entityDocument = await customerDatabase.FindEntity<TradingLocation>(LookupPredicate.Create(tradingLocationId, customerId, legalEntityId));
            if (entityDocument == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return MessageEnvelop_ToEntityResponse_TradingLocation.Convert(entityDocument);
        }

        [HttpDelete("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}")]
        public async Task<StatusCodeResult> RemoveTradingLocation([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId)
        {
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

            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}/submit")]
        public async Task<ActionResult<ApiContract.SubmitActionResponse>> SubmitTradingLocation([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId, [FromBody] ApiContract.SubmitActionRequest submitActionRequest)
        {
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

            return new ApiContract.SubmitActionResponse()
            {
                Entity_id = legalEntityId,
                Submitted_version = (long)result.SubmittedVersion
            };
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}/touch")]
        public async Task<StatusCodeResult> TouchTradingLocation([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId)
        {
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

            return new NoContentResult();
        }

        [HttpPatch("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_TradingLocation>> UpdateTradingLocation([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId, ApiContract.UpdateTradingLocation updateTradingLocation)
        {
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

            return await GetTradingLocationById(customerId, legalEntityId, tradingLocationId);
        }
    }
}

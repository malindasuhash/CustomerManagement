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

        [HttpGet("trading-locations/find-by-contact")]
        public async Task<ActionResult<List<EntityDocumentModel>>> FindTradingLocationsByContact([FromQuery] string customerId, [FromQuery] string contactId)
        {
            if (string.IsNullOrWhiteSpace(customerId) || string.IsNullOrWhiteSpace(contactId))
            {
                return BadRequest("customerId and contactId are required query parameters.");
            }

            var envelopes = await customerDatabase.GetTradingLocationsBy(customerId, contactId);

            var results = envelopes?.Select(e => Translate(e)).ToList() ?? new List<EntityDocumentModel>();

            return Ok(results);
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
        public async Task<ActionResult<EntityDocumentModel>> RemoveTradingLocation([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId)
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
        public async Task<ActionResult<EntityDocumentModel>> TouchTradingLocation([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId)
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

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
        public async Task<ActionResult<EntityDocumentModel>> CreateTradingLocation(string customerId, string legalEntityId, TradingLocation tradingLocation)
        {
            tradingLocation.LegalEntityId = legalEntityId;

            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.ProductAgreement,
                Draft = tradingLocation,
                CustomerId = customerId
            };

            return await Process<TradingLocation>(envelop);
        }

        [HttpGet("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}")]
        public async Task<ActionResult<EntityDocumentModel>> GetTradingLocationById([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId)
        {
            return await GetById<TradingLocation>(LookupPredicate.Create(tradingLocationId, customerId, legalEntityId));
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

            return await Process<TradingLocation>(envelop);
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}/submit")]
        public async Task<ActionResult<EntityDocumentModel>> SubmitTradingLocation([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId, SubmitEntityModel submitEntityModel)
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
                DraftVersion = submitEntityModel.TargetVersion
            };

            return await Process<TradingLocation>(envelop);
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

            return await Process<TradingLocation>(envelop);
        }

        [HttpPatch("{customerId}/legal-entities/{legalEntityId}/trading-locations/{tradingLocationId}")]
        public async Task<ActionResult<EntityDocumentModel>> UpdateTradingLocation([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string tradingLocationId, TradingLocationModel patchModel)
        {
            var patch = TradingLocationModelToPatch(patchModel, legalEntityId);

            var envelop = new MessageEnvelop
            {
                EntityId = tradingLocationId,
                Change = ChangeType.Update,
                Name = EntityName.TradingLocation,
                Draft = patch,
                CustomerId = customerId,
                DraftVersion = patchModel.TargetVersion
            };

            return await Process<TradingLocation>(envelop);
        }

        private static TradingLocation TradingLocationModelToPatch(TradingLocationModel patchModel, string legalEntityId)
        {
            var tradingLocation = new TradingLocation
            {
                LegalEntityId = legalEntityId,
                Name = patchModel.Name,
                Website = patchModel.Website,
                Label = patchModel.Label
            };

            if (patchModel.Contacts != null)
            {
                tradingLocation.Contacts = [.. patchModel.Contacts.Select(a => new ContactReference() { ContactId = a.ContactId, ContactType = Enum.Parse<ContactType>(a.ContactType, true) })];
            }

            if (patchModel.Address != null)
            {
                tradingLocation.Address = new Address()
                {
                    Code = patchModel.Address.Code,
                    Country = patchModel.Address.Country,
                    Line1 = patchModel.Address.Line1,
                    Line2 = patchModel.Address.Line2,
                    Line3 = patchModel.Address.Line3,
                    Locality = patchModel.Address.Locality,
                    Name = patchModel.Address.Name,
                    Region = patchModel.Address.Region
                };
            }

            if (patchModel.Descriptors != null)
            {
                //tradingLocation.Descriptors = [.. patchModel.Descriptors.Select(a => new MetaData() { Key = a.Key, Value = a.Value })];
            }

            return tradingLocation;
        }
    }
}

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
    public class LegalEntityController : EntityManagementController
    {
        public LegalEntityController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase) : base(changeProcessor, customerDatabase)
        {
        }

        [HttpGet("legal-entities/find-by-contact")]
        public async Task<ActionResult<List<EntityDocumentModel>>> FindLegalEntitiesByContact([FromQuery] string customerId, [FromQuery] string contactId)
        {
            if (string.IsNullOrWhiteSpace(customerId) || string.IsNullOrWhiteSpace(contactId))
            {
                return BadRequest("customerId and contactId are required query parameters.");
            }

            var envelopes = await customerDatabase.GetLegalEntitiesBy(customerId, contactId);

            var results = envelopes?.Select(e => Translate(e)).ToList() ?? new List<EntityDocumentModel>();

            return Ok(results);
        }

        [HttpPost("{customerId}/legal-entities/{entityId}/touch")]
        public async Task<ActionResult<EntityDocumentModel>> TouchLegalEntity([FromRoute] string customerId, [FromRoute] string entityId)
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

            return await Process<LegalEntity>(envelop);
        }

        [HttpPost("{customerId}/legal-entities/{entityId}/submit")]
        public async Task<ActionResult<EntityDocumentModel>> SubmitLegalEntity([FromRoute] string customerId, [FromRoute] string entityId, [FromBody] SubmitEntityModel submitModel)
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
                DraftVersion = submitModel.TargetVersion
            };

            return await Process<LegalEntity>(envelop);
        }

        [HttpDelete("{customerId}/legal-entities/{entityId}")]
        public async Task<ActionResult<EntityDocumentModel>> RemoveLegalEntity([FromRoute] string customerId, [FromRoute] string entityId)
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

            return await Process<LegalEntity>(envelop);
        }

        [HttpPost("{customerId}/legal-entities")]
        public async Task<ActionResult<EntityDocumentModel>> CreateLegalEntity([FromRoute] string customerId, [FromBody] LegalEntity legalEntity)
        {
            // LEGAL_ENTITY_WRITE
            // LEGAL_ENTITY_READ
            // SYSTEM_DATA_READ
            // SOFTDELETE_DATA_READ
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.BankAccount,
                Draft = legalEntity,
                CustomerId = customerId
            };

            return await Process<LegalEntity>(envelop);
        }

        [HttpGet("{customerId}/legal-entities/{entityId}")]
        public async Task<ActionResult<EntityDocumentModel>> GetLegalEntityById(string customerId, string entityId)
        {
            // LEGAL_ENTITY_READ
            // SYSTEM_DATA_READ
            // SOFTDELETE_DATA_READ
            return await GetById<LegalEntity>(LookupPredicate.Create(entityId, customerId));
        }

        [HttpPatch("{customerId}/legal-entities/{entityId}")]
        public async Task<ActionResult<EntityDocumentModel>> UpdateLegalEntity([FromRoute] string customerId, [FromRoute] string entityId, [FromBody] LegalEntityModel patch)
        {
            // LEGAL_ENTITY_UPDATE
            // LEGAL_ENTITY_READ
            // SYSTEM_DATA_READ
            // SOFTDELETE_DATA_READ
            var patchModel = LegalEntityToPatch(patch);

            var envelop = new MessageEnvelop
            {
                EntityId = entityId,
                Change = ChangeType.Update,
                Name = EntityName.LegalEntity,
                Draft = patchModel,
                CustomerId = customerId,
                DraftVersion = patch.TargetVersion
            };

            return await Process<LegalEntity>(envelop);
        }

        private static LegalEntity LegalEntityToPatch(LegalEntityModel patch)
        {
            var legalEntity = new LegalEntity()
            {
                BusinessEmail = patch.BusinessEmail,
                BusinessType = patch.BusinessType,
                CardTurnoverPerAnnum = patch.CardTurnoverPerAnnum,
                CompanyRegistration = patch.CompanyRegistration,
                DateBusinessStarted = !string.IsNullOrWhiteSpace(patch.DateBusinessStarted) ? DateTime.Parse(patch.DateBusinessStarted) : null,
                DateTradingStarted = !string.IsNullOrWhiteSpace(patch.DateTradingStarted) ? DateTime.Parse(patch.DateTradingStarted) : null,
                Label = patch.Label,
                MaximumTransactionValue = patch.MaximumTransactionValue,
                MerchantCategoryCode = patch.MerchantCategoryCode,
                Name = patch.Name,
                StandardIndustryClassification = patch.StandardIndustryClassification,
                TradingName = patch.TradingName,
                TurnoverPerAnnum = patch.TurnoverPerAnnum,
                VatRegistration = patch.VatRegistration,
                VatRegistrationStatus = patch.VatRegistrationStatus
            };

            if (patch.BusinessContacts != null)
            {
                legalEntity.BusinessContacts = [.. patch.BusinessContacts.Select(x => new BusinessContact()
                {
                    ContactId = x.ContactId,
                    ContactType = Enum.Parse<ContactType>(x.ContactType, true)
                })];
            }

            if (patch.Descriptors != null)
            {
                legalEntity.Descriptors = [.. patch.Descriptors.Select(x => new Descriptor() { Key = x.Key, Value = x.Value })];
            }

            if (patch.LegalEntitiesWithControl != null)
            {
                legalEntity.LegalEntitiesWithControl = [.. patch.LegalEntitiesWithControl.Select(x => new LegalEntityWithControl()
                {
                    LegalEntityId = x.LegalEntityId,
                    ControlTypes = [.. x.ControlTypes.Select(y => Enum.Parse<ControlType>(y))]
                })];
            }

            if (patch.PersonsWithControl != null)
            {
                legalEntity.PersonsWithControl = [.. patch.PersonsWithControl.Select(x => new PersonWithControl()
                {
                    ControlTypes = [.. x.ControlTypes.Select(y => Enum.Parse<ControlType>(y))],
                    Person = new Person() {
                        Title = x.Person.Title,
                        Nationality = x.Person.Nationality,
                        FirstName = x.Person.FirstName,
                        LastName = x.Person.LastName,
                        DateOfBirth = x.Person.DateOfBirth,
                        MiddleName = x.Person.MiddleName,
                        Address = new Address()
                        {
                            Code = x.Person.Address.Code,
                            Country = x.Person.Address.Country,
                            Line1 = x.Person.Address.Line1,
                            Line2 = x.Person.Address.Line2,
                            Line3 = x.Person.Address.Line3,
                            Locality = x.Person.Address.Locality,
                            Name = x.Person.Address.Name,
                            Region = x.Person.Address.Region
                        }
                    }
                })];
            }

            if (patch.RegisteredAddresses != null)
            {
                legalEntity.RegisteredAddresses = [.. patch.RegisteredAddresses.Select(x => new RegisteredAddress()
                {
                    Current = x.Current,
                    DateFrom = DateTime.Parse(x.DateFrom),
                    DateTo = x.DateTo != null ? DateTime.Parse(x.DateTo) : null,
                    Address = new Address()
                    {
                            Code = x.Address.Code,
                            Country = x.Address.Country,
                            Line1 = x.Address.Line1,
                            Line2 = x.Address.Line2,
                            Line3 = x.Address.Line3,
                            Locality = x.Address.Locality,
                            Name = x.Address.Name,
                            Region = x.Address.Region
                    }
                }) ];
            }

            return legalEntity;
        }
    }
}

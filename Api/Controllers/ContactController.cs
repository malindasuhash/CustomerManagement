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
    public class ContactController : EntityManagementController
    {
        public ContactController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase) : base(changeProcessor, customerDatabase)
        {
        }

        [HttpPost("{customerId}/contacts/{contactId}/touch")]
        public async Task<ActionResult<EntityDocumentModel>> TouchContact([FromRoute] string customerId, [FromRoute] string contactId)
        {
            var envelop = new MessageEnvelop()
            {
                Name = EntityName.Contact,
                CustomerId = customerId,
                EntityId = contactId,
                Change = ChangeType.Touch
            };

           return await Touch<Contact>(envelop);
        }

        [HttpPost("{customerId}/contacts/{contactId}/submit")]
        public async Task<ActionResult<EntityDocumentModel>> SubmitContact([FromRoute] string customerId, [FromRoute] string contactId, [FromBody] SubmitEntityModel submitModel)
        {
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Submit,
                Name = EntityName.Contact,
                CustomerId = customerId,
                EntityId = contactId,
                IsSubmitted = true,
                DraftVersion = submitModel.TargetVersion
            };

            return await Submit<Contact>(envelop);
        }

        [HttpDelete("{customerId}/contacts/{contactId}")]
        public async Task<ActionResult<EntityDocumentModel>> RemoveContact([FromRoute] string customerId, [FromRoute] string contactId)
        {
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Delete,
                Name = EntityName.Contact,
                CustomerId = customerId,
                EntityId = contactId,
            };

            return await Remove<Contact>(envelop);
        }

        [HttpPost("{customerId}/contacts")]
        public async Task<ActionResult<EntityDocumentModel>> CreateContact([FromRoute] string customerId, [FromBody] Contact contact)
        {
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.Contact,
                Draft = contact,
                CustomerId = customerId
            };

            return await Create<Contact>(envelop);
        }

        [HttpGet("{customerId}/contacts/{contactId}")]
        public async Task<ActionResult<EntityDocumentModel>> GetContactById(string customerId, string contactId)
        {
            return await GetById<Contact>(customerId, contactId);
        }

        [HttpPatch("{customerId}/contacts/{contactId}")]
        public async Task<ActionResult<EntityDocumentModel>> UpateContact([FromRoute] string customerId, [FromRoute] string contactId, [FromBody] ContactModel patch)
        {
            var patchModel = ContactToPatch(patch);
            var envelop = new MessageEnvelop
            {
                EntityId = contactId,
                Change = ChangeType.Update,
                Name = EntityName.Contact,
                Draft = patchModel,
                CustomerId = customerId,
                DraftVersion = patch.TargetVersion
            };

            await changeProcessor.ProcessChangeAsync<Contact>(envelop);

            var contactEntity = await customerDatabase.GetEntity<Contact>(customerId, contactId);

            return Translate(contactEntity);
        }

        private static Contact ContactToPatch(ContactModel patchModel)
        {
            // There must be a better way to map from a api model to a domain model.
            // Number of other properties are ignored for now. Keen to get the concept 
            // operational.
            var contact = new Contact
            {
                FirstName = patchModel.FirstName,
                LastName = patchModel.LastName,
                AltTelephone = patchModel.AltTelephone,
                AltTelephoneCode = patchModel.AltTelephoneCode,
                Email = patchModel.Email,
                Label = patchModel.Label,
                Telephone = patchModel.Telephone,
                TelephoneCode = patchModel.TelephoneCode
            };

            if (patchModel.Descriptors != null)
            {
                contact.Descriptors = [.. patchModel.Descriptors.Select(a => new Descriptor() { Key = a.Key, Value = a.Value })];
            }

            if (patchModel.PostalAddress != null)
            {
                contact.PostalAddress = new Address()
                {
                    Code = patchModel.PostalAddress.Code,
                    Country = patchModel.PostalAddress.Country,
                    Line1 = patchModel.PostalAddress.Line1,
                    Line2 = patchModel.PostalAddress.Line2,
                    Line3 = patchModel.PostalAddress.Line3,
                    Locality = patchModel.PostalAddress.Locality,
                    Name = patchModel.PostalAddress.Name,
                    Region = patchModel.PostalAddress.Region
                };
            }

            return contact;
        }
    }
}

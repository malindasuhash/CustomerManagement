using Api.ApiModels;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
using StateManagment.Models;
using StateManagment.Services;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/customers")]
    public class ContactController : ControllerBase
    {
        private readonly CustomerManagementService contactService;

        public ContactController(CustomerManagementService contactService)
        {
            this.contactService = contactService;
        }

        [HttpPost("{customerId}/contact/{contactId}/touch")]
        public async Task<ActionResult<EntityDocumentModel>> TouchContact([FromRoute] string customerId, [FromRoute] string contactId)
        {
            // Authorisation layer may go here
            var result = await contactService.Touch(EntityName.Contact, customerId, contactId);

            if (result != TaskOutcome.OK) 
            { 
                return BadRequest(result);
            }

            var contactEntity = await contactService.Get(EntityName.Contact, customerId, contactId);

            return Translate(contactEntity);
        }

         [HttpPost("{customerId}/contact/{contactId}/submit")]
        public async Task<ActionResult<EntityDocumentModel>> SubmitContact([FromRoute] string customerId, [FromRoute] string contactId, [FromBody] SubmitEntityModel submitModel)
        {
            var result = await contactService.Submit(EntityName.Contact, customerId, contactId, submitModel.TargetVersion);

            if (result != TaskOutcome.OK)
            {
                return BadRequest(result);
            }

            var contactEntity = await contactService.Get(EntityName.Contact, customerId, contactId);

            return Translate(contactEntity);
        }

        [HttpDelete("{customerId}/contact/{contactId}")]
        public async Task<ActionResult<EntityDocumentModel>> RemoveContact([FromRoute] string customerId, [FromRoute] string contactId)
        {
            await contactService.Delete(EntityName.Contact, customerId, contactId, false);

            var contactEntity = await contactService.Get(EntityName.Contact, customerId, contactId);

            return Translate(contactEntity);
        }

        [HttpPost("{customerId}/contact")]
        public async Task<ActionResult<EntityDocumentModel>> CreateContact([FromRoute] string customerId, [FromBody] Contact contact)
        {
            var storedEntity = await contactService.Post(contact, EntityName.Contact, customerId, false);

            var contactEntity = await contactService.Get(EntityName.Contact, storedEntity.CustomerId, storedEntity.EntityId);

            return Translate(contactEntity);
        }

        [HttpGet("{customerId}/contact/{contactId}")]
        public async Task<ActionResult<EntityDocumentModel>> GetContactById(string customerId, string contactId)
        {
            var contact = await contactService.Get(EntityName.Contact, customerId, contactId);

            return Translate(contact);
        }

        [HttpPatch("{customerId}/contact/{contactId}")]
        public async Task<ActionResult<EntityDocumentModel>> UpateContact([FromRoute] string customerId, [FromRoute] string contactId, [FromBody] ContactEntityModel patch)
        {
            var patchModel = ContactToPatch(patch);
            await contactService.Patch(patchModel, EntityName.Contact, customerId, contactId, patch.TargetVersion, false);

            var contactEntity = await contactService.Get(EntityName.Contact, customerId, contactId);

            return Translate(contactEntity);
        }

        private static Contact ContactToPatch(ContactEntityModel patchModel)
        {
            // There must be a better way to map from a view model to a domain model.
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

        private EntityDocumentModel Translate(MessageEnvelop contact)
        {
            var model = new EntityDocumentModel()
            {
                CustomerId = contact.CustomerId,
                EntityId = contact.EntityId,
                Submitted = contact.Submitted,
                SubmittedVersion = contact.SubmittedVersion,
                Applied = contact.Applied,
                AppliedVersion = contact.AppliedVersion,
                CreatedTimestamp = contact.CreatedTimestamp,
                CreatedUser = contact.CreatedUser,
                Draft = contact.Draft,
                DraftVersion = contact.DraftVersion,
                State = contact.State.ToString(),
                Feedback = contact.Feedback,
                Removed = contact.Removed,
                RemoveRequested = contact.RemoveRequested
            };

            return model;
        }

    }
}

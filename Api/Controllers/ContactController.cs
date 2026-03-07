using Api.ApiModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
using StateManagment.Models;
using StateManagment.Services;
using System.Linq;

namespace Api.Controllers
{
    [ApiController]
    [Route("")]
    public class ContactController : ControllerBase
    {
        private readonly ContactService contactService;

        public ContactController(ContactService contactService)
        {
            this.contactService = contactService;
        }

        [HttpPost("customers/{customerId}/contact")]
        public async Task<EntityDocumentModel> CreateContact([FromRoute] string customerId, [FromBody] Contact contact)
        {
            var storedEntity = await contactService.Post(customerId, contact, false);

            var contactEntity = await contactService.Get(storedEntity.CustomerId, storedEntity.EntityId);

            return Translate(contactEntity);
        }

        [HttpGet("customers/{customerId}/contact/{contactId}")]
        public async Task<EntityDocumentModel> GetContactById(string customerId, string contactId)
        {
            var contact = await contactService.Get(customerId, contactId);

            return Translate(contact);
        }

        [HttpPatch("customers/{customerId}/contact/{contactId}")]
        public async Task<EntityDocumentModel> UpateContact([FromRoute] string customerId, [FromRoute] string contactId, [FromBody] PatchContactModel patch)
        {
            var patchModel = ContactToPatch(patch);
            await contactService.Patch(patchModel, customerId, contactId, patch.TargetVersion, false);

            var contactEntity = await contactService.Get(customerId, contactId);

            return Translate(contactEntity);
        }

        private static Contact ContactToPatch(PatchContactModel patchModel)
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
            };

            return model;
        }

    }
}

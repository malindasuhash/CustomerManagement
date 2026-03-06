using Api.ApiModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
using StateManagment.Models;
using StateManagment.Services;

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

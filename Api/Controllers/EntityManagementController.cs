using Api.ApiModels;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Models;

namespace Api.Controllers
{
    public abstract class EntityManagementController : ControllerBase
    {
        protected readonly IChangeProcessor changeProcessor;
        protected readonly ICustomerDatabase customerDatabase;

        protected EntityManagementController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase)
        {
            this.changeProcessor = changeProcessor;
            this.customerDatabase = customerDatabase;
        }

        internal async Task<ActionResult<EntityDocumentModel>> GetById(EntityName entityName, string customerId, string entityId)
        {
            var contact = await customerDatabase.GetEntityDocument(entityName, entityId, customerId);

            return Translate(contact);
        }

        internal async Task<ActionResult<EntityDocumentModel>> Create(MessageEnvelop envelop)
        {
            await changeProcessor.ProcessChangeAsync(envelop);

            var specificEntity = await customerDatabase.GetEntityDocument(envelop.Name, envelop.EntityId, envelop.CustomerId);

            return Translate(specificEntity);
        }

        internal async Task<ActionResult<EntityDocumentModel>> Touch(MessageEnvelop messageEnvelop)
        {
            // Authorisation layer may go here
            var result = await changeProcessor.ProcessChangeAsync(messageEnvelop);

            if (result != TaskOutcome.OK)
            {
                return BadRequest(result);
            }

            var specificEntity = await customerDatabase.GetEntityDocument(messageEnvelop.Name, messageEnvelop.EntityId, messageEnvelop.CustomerId);

            return Translate(specificEntity);
        }

        internal async Task<ActionResult<EntityDocumentModel>> Submit(MessageEnvelop envelop)
        {
            var result = await changeProcessor.ProcessChangeAsync(envelop);

            if (result != TaskOutcome.OK)
            {
                return BadRequest(result);
            }

            var contactEntity = await customerDatabase.GetEntityDocument(envelop.Name, envelop.EntityId, envelop.CustomerId);

            return Translate(contactEntity);
        }

        internal async Task<ActionResult<EntityDocumentModel>> Remove(MessageEnvelop envelop)
        {
            await changeProcessor.ProcessChangeAsync(envelop);

            var contactEntity = await customerDatabase.GetEntityDocument(envelop.Name, envelop.EntityId, envelop.CustomerId);

            return Translate(contactEntity);
        }

        protected EntityDocumentModel Translate(MessageEnvelop messageEnvelop)
        {
            var model = new EntityDocumentModel()
            {
                CustomerId = messageEnvelop.CustomerId,
                EntityId = messageEnvelop.EntityId,
                Submitted = messageEnvelop.Submitted,
                SubmittedVersion = messageEnvelop.SubmittedVersion,
                Applied = messageEnvelop.Applied,
                AppliedVersion = messageEnvelop.AppliedVersion,
                CreatedTimestamp = messageEnvelop.CreatedTimestamp,
                CreatedUser = messageEnvelop.CreatedUser,
                Draft = messageEnvelop.Draft,
                DraftVersion = messageEnvelop.DraftVersion,
                State = messageEnvelop.State.ToString(),
                Feedback = messageEnvelop.Feedback,
                Removed = messageEnvelop.Removed,
                RemoveRequested = messageEnvelop.RemoveRequested
            };

            return model;
        }

    }
}

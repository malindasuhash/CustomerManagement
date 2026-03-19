using Api.ApiModels;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
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

        internal async Task<ActionResult<EntityDocumentModel>> GetById<T>(LookupPredicate lookupPredicate) where T : IEntity
        {
            var contact = await customerDatabase.FindEntity<T>(lookupPredicate);

            return Translate(contact);
        }

        internal async Task<ActionResult<EntityDocumentModel>> Create<T>(MessageEnvelop envelop) where T : IEntity
        {
            await changeProcessor.ProcessChangeAsync<T>(envelop);

            var specificEntity = await customerDatabase.FindEntity<T>(envelop.SearchBy());

            return Translate(specificEntity);
        }

        internal async Task<ActionResult<EntityDocumentModel>> Touch<T>(MessageEnvelop messageEnvelop) where T : IEntity
        {
            // Authorisation layer may go here
            var result = await changeProcessor.ProcessChangeAsync<T>(messageEnvelop);

            if (result != TaskOutcome.OK)
            {
                return BadRequest(result);
            }

            var specificEntity = await customerDatabase.FindEntity<T>(messageEnvelop.SearchBy());

            return Translate(specificEntity);
        }

        internal async Task<ActionResult<EntityDocumentModel>> Submit<T>(MessageEnvelop envelop) where T : IEntity
        {
            var result = await changeProcessor.ProcessChangeAsync<T>(envelop);

            if (result != TaskOutcome.OK)
            {
                return BadRequest(result);
            }

            var contactEntity = await customerDatabase.FindEntity<T>(envelop.SearchBy());

            return Translate(contactEntity);
        }

        internal async Task<ActionResult<EntityDocumentModel>> Remove<T>(MessageEnvelop envelop) where T : IEntity
        {
            await changeProcessor.ProcessChangeAsync<T>(envelop);

            var contactEntity = await customerDatabase.FindEntity<T>(envelop.SearchBy());

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

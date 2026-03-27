using Api.ApiModels;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Controllers
{
    public abstract class EntityManagementController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase) : ControllerBase
    {
        protected readonly IChangeProcessor changeProcessor = changeProcessor;
        protected readonly ICustomerDatabase customerDatabase = customerDatabase;

        protected async Task<ActionResult<EntityDocumentModel>> GetById<T>(LookupPredicate lookupPredicate) where T : IEntity
        {
            var entityDocument = await customerDatabase.FindEntity<T>(lookupPredicate);

            if (entityDocument == MessageEnvelop.NONE)
            {
                return NotFound(TaskOutcome.NOT_FOUND);
            }

            return Translate(entityDocument);
        }

        protected async Task<ActionResult<EntityDocumentModel>> Process<T>(MessageEnvelop messageEnvelop) where T : IEntity
        {
            var result = await changeProcessor.ProcessChangeAsync<T>(messageEnvelop);

            if (result != TaskOutcome.OK)
            {
                return BadRequest(result);
            }

            return await GetById<T>(messageEnvelop.SearchBy());
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

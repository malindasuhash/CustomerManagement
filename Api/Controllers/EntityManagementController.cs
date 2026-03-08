using Api.ApiModels;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
using StateManagment.Models;
using StateManagment.Services;

namespace Api.Controllers
{
    public abstract class EntityManagementController : ControllerBase
    {
        private readonly CustomerManagementService contactService;

        public EntityManagementController(CustomerManagementService contactService)
        {
            this.contactService = contactService;
        }

        internal async Task<ActionResult<EntityDocumentModel>> GetById(EntityName entityName, string customerId, string contactId)
        {
            var contact = await contactService.Get(entityName, customerId, contactId);

            return Translate(contact);
        }

        internal async Task<ActionResult<EntityDocumentModel>> Create<T>(EntityName entityName, string customerId, T entity) where T : IEntity, new()
        {
            var storedEntity = await contactService.Post(entity, entityName, customerId, false);

            var specificEntity = await contactService.Get(entityName, storedEntity.CustomerId, storedEntity.EntityId);

            return Translate(specificEntity);
        }

        [HttpPost("{customerId}/contact/{contactId}/touch")]
        internal async Task<ActionResult<EntityDocumentModel>> Touch(EntityName entityName, string customerId, string contactId)
        {
            // Authorisation layer may go here
            var result = await contactService.Touch(entityName, customerId, contactId);

            if (result != TaskOutcome.OK)
            {
                return BadRequest(result);
            }

            var specificEntity = await contactService.Get(entityName, customerId, contactId);

            return Translate(specificEntity);
        }

        internal async Task<ActionResult<EntityDocumentModel>> Submit(EntityName entityName, string customerId, string entityId, SubmitEntityModel submitModel)
        {
            var result = await contactService.Submit(entityName, customerId, entityId, submitModel.TargetVersion);

            if (result != TaskOutcome.OK)
            {
                return BadRequest(result);
            }

            var contactEntity = await contactService.Get(entityName, customerId, entityId);

            return Translate(contactEntity);
        }

        internal async Task<ActionResult<EntityDocumentModel>> Remove(EntityName entityName, string customerId, string entityId)
        {
            await contactService.Delete(entityName, customerId, entityId, false);

            var contactEntity = await contactService.Get(entityName, customerId, entityId);

            return Translate(contactEntity);
        }

        protected abstract EntityDocumentModel Translate(MessageEnvelop messageEnvelop);
        
    }
}

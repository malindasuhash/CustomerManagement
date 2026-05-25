using Api.Mappers;
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
        private readonly ILogger<ContactController> logger;

        public ContactController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase, ILogger<ContactController> logger) : base(changeProcessor, customerDatabase, logger)
        {
            this.logger = logger;
        }

        [HttpPost("{customerId}/contacts/{contactId}/touch")]
        public async Task<StatusCodeResult> TouchContact([FromRoute] string customerId, [FromRoute] string contactId)    
        {
            logger.LogInformation("Touching contact with id {ContactId} for customer {CustomerId}", contactId, customerId);
            var envelop = new MessageEnvelop()
            {
                Name = EntityName.Contact,
                CustomerId = customerId,
                EntityId = contactId,
                Change = ChangeType.Touch
            };

            var result = await SubmitForProcessing<Contact>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("Successfully touched contact with id {ContactId} for customer {CustomerId}", contactId, customerId);
            return new NoContentResult();
        }

        [HttpPost("{customerId}/contacts/{contactId}/submit")]
        public async Task<ActionResult<ApiContract.SubmitActionResponse>> SubmitContact([FromRoute] string customerId, [FromRoute] string contactId, [FromBody] ApiContract.SubmitActionRequest submitActionRequest)
        {
            logger.LogInformation("Submitting contact with id {ContactId} for customer {CustomerId}, TargetDraftVersion={TargetDraftVersion}", contactId, customerId, submitActionRequest?.Target_draft_version);
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Submit,
                Name = EntityName.Contact,
                CustomerId = customerId,
                EntityId = contactId,
                IsSubmitted = true,
                DraftVersion = submitActionRequest.Target_draft_version
            };

            var result = await SubmitForProcessing<Contact>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("Successfully submitted contact with id {ContactId} for customer {CustomerId}, SubmittedVersion={SubmittedVersion}", contactId, customerId, result.SubmittedVersion);
            return new ApiContract.SubmitActionResponse()
            {
                Entity_id = contactId,
                Submitted_version = (long)result.SubmittedVersion
            };
        }

        [HttpDelete("{customerId}/contacts/{contactId}")]
        public async Task<StatusCodeResult> RemoveContact([FromRoute] string customerId, [FromRoute] string contactId)
        {
            logger.LogInformation("Removing contact with id {ContactId} for customer {CustomerId}", contactId, customerId);
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Delete,
                Name = EntityName.Contact,
                CustomerId = customerId,
                EntityId = contactId,
            };

            var result = await SubmitForProcessing<Contact>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("Successfully removed contact with id {ContactId} for customer {CustomerId}", contactId, customerId);
            return new NoContentResult();
        }

        [HttpPost("{customerId}/contacts")]
        public async Task<ActionResult<ApiContract.EntityResponse_Contact>> CreateContact([FromRoute] string customerId, [FromBody] ApiContract.CreateContact apiContact)
        {
            logger.LogInformation("Creating contact for customer {CustomerId}", customerId);
            var domainContact = ApiContractContact_ToModelContactMap.Convert(apiContact);

            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.Contact,
                Draft = domainContact,
                CustomerId = customerId
            };

            var result = await SubmitForProcessing<Contact>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("Successfully created contact with id {ContactId} for customer {CustomerId}", result.EntityId, customerId);

            return await GetContactById(customerId, envelop.EntityId);
        }

        [HttpGet("{customerId}/contacts/{contactId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_Contact>> GetContactById(string customerId, string contactId)
        {
            logger.LogInformation("Getting contact with id {ContactId} for customer {CustomerId}", contactId, customerId);

            var entityDocument = await customerDatabase.FindEntity<Contact>(LookupPredicate.Create(contactId, customerId));
            if (entityDocument == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("Successfully got contact with id {ContactId} for customer {CustomerId}", contactId, customerId);
            return MessageEnvelop_ToEntityResponse_Contact.Convert(entityDocument);
        }

        [HttpPatch("{customerId}/contacts/{contactId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_Contact>> UpateContact([FromRoute] string customerId, [FromRoute] string contactId, [FromBody] ApiContract.UpdateContact patch)
        {
            logger.LogInformation("Updating contact with id {ContactId} for customer {CustomerId}, TargetDraftVersion={TargetDraftVersion}", contactId, customerId, patch?.Target_draft_version);
            Contact patchContact = ApiContractContact_ToModelContactMap.Update(patch);
            var envelop = new MessageEnvelop
            {
                EntityId = contactId,
                Change = ChangeType.Update,
                Name = EntityName.Contact,
                Draft = patchContact,
                CustomerId = customerId,
                DraftVersion = (decimal)patch.Target_draft_version
            };

            var result = await SubmitForProcessing<Contact>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation("Successfully updated contact with id {ContactId} for customer {CustomerId}, NewDraftVersion={NewDraftVersion}", contactId, customerId, result.DraftVersion);

            return await GetContactById(customerId, contactId);
        }
    }
}

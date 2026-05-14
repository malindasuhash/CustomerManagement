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
        public ContactController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase) : base(changeProcessor, customerDatabase)
        {
        }

        [HttpPost("{customerId}/contacts/{contactId}/touch")]
        public async Task<StatusCodeResult> TouchContact([FromRoute] string customerId, [FromRoute] string contactId)    
        {
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

            return new NoContentResult();
        }

        [HttpPost("{customerId}/contacts/{contactId}/submit")]
        public async Task<ActionResult<ApiContract.SubmitActionResponse>> SubmitContact([FromRoute] string customerId, [FromRoute] string contactId, [FromBody] ApiContract.SubmitActionRequest submitActionRequest)
        {
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

            return new ApiContract.SubmitActionResponse()
            {
                Entity_id = contactId,
                Submitted_version = (long)result.SubmittedVersion
            };
        }

        [HttpDelete("{customerId}/contacts/{contactId}")]
        public async Task<StatusCodeResult> RemoveContact([FromRoute] string customerId, [FromRoute] string contactId)
        {
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

            return new NoContentResult();
        }

        [HttpPost("{customerId}/contacts")]
        public async Task<ActionResult<ApiContract.EntityResponse_Contact>> CreateContact([FromRoute] string customerId, [FromBody] ApiContract.CreateContact apiContact)
        {
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

            return await GetContactById(customerId, envelop.EntityId);
        }

        [HttpGet("{customerId}/contacts/{contactId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_Contact>> GetContactById(string customerId, string contactId)
        {
            var entityDocument = await customerDatabase.FindEntity<Contact>(LookupPredicate.Create(contactId, customerId));
            if (entityDocument == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return MessageEnvelop_ToEntityResponse_Contact.Convert(entityDocument);
        }

        [HttpPatch("{customerId}/contacts/{contactId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_Contact>> UpateContact([FromRoute] string customerId, [FromRoute] string contactId, [FromBody] ApiContract.UpdateContact patch)
        {
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

            return await GetContactById(customerId, contactId);
        }
    }
}

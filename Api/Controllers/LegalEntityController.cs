using Api.ApiModels;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
using StateManagment.Models;
using StateManagment.Services;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/customers")]
    public class LegalEntityController : EntityManagementController
    {
        public LegalEntityController(CustomerManagementService contactService) : base(contactService)
        {
        }

        [HttpPost("{customerId}/legal-entities/{entityId}/touch")]
        public async Task<ActionResult<EntityDocumentModel>> TouchLegalEntity([FromRoute] string customerId, [FromRoute] string entityId)
        {
            return await Touch(EntityName.LegalEntity, customerId, entityId);
        }

        [HttpPost("{customerId}/legal-entities/{entityId}/submit")]
        public async Task<ActionResult<EntityDocumentModel>> SubmitLegalEntity([FromRoute] string customerId, [FromRoute] string entityId, [FromBody] SubmitEntityModel submitModel)
        {
            return await Submit(EntityName.LegalEntity, customerId, entityId, submitModel);
        }

        [HttpDelete("{customerId}/legal-entities/{entityId}")]
        public async Task<ActionResult<EntityDocumentModel>> RemoveLegalEntity([FromRoute] string customerId, [FromRoute] string entityId)
        {
            return await Remove(EntityName.LegalEntity, customerId, entityId);
        }

        [HttpPost("{customerId}/legal-entities")]
        public async Task<ActionResult<EntityDocumentModel>> CreateLegalEntity([FromRoute] string customerId, [FromBody] LegalEntity legalEntity)
        {
            return await Create(EntityName.LegalEntity, customerId, legalEntity);
        }

        [HttpGet("{customerId}/legal-entities/{entityId}")]
        public async Task<ActionResult<EntityDocumentModel>> GetLegalEntityById(string customerId, string entityId)
        {
            return await GetById(EntityName.LegalEntity, customerId, entityId);
        }
    }
}

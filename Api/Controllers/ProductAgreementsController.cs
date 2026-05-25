using Api.ApiModels;
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
    public class ProductAgreementsController : EntityManagementController
    {
        private readonly ILogger<ProductAgreementsController> logger;

        public ProductAgreementsController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase, ILogger<ProductAgreementsController> logger) : base(changeProcessor, customerDatabase, logger)
        {
            this.logger = logger;
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}/touch")]
        public async Task<StatusCodeResult> TouchProductAgreement([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId)
        {
            logger.LogInformation($"Touching product agreement {productAgreementId} for legal entity {legalEntityId} of customer {customerId}");

            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Touch,
                Name = EntityName.ProductAgreement,
                CustomerId = customerId,
                EntityId = productAgreementId,
                Draft = new ProductAgreement()
                {
                    LegalEntityId = legalEntityId
                }
            };

            var result = await SubmitForProcessing<ProductAgreement>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"Successfully touched product agreement {productAgreementId} for legal entity {legalEntityId} of customer {customerId}");
            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}/submit")]
        public async Task<ActionResult<ApiContract.SubmitActionResponse>> SubmitProductAgreement([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId, [FromBody] ApiContract.SubmitActionRequest submitActionRequest)
        {
            logger.LogInformation($"Submitting product agreement {productAgreementId} for legal entity {legalEntityId} of customer {customerId} with target draft version {submitActionRequest.Target_draft_version}");
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Submit,
                Name = EntityName.ProductAgreement,
                CustomerId = customerId,
                EntityId = productAgreementId,
                IsSubmitted = true,
                Draft = new ProductAgreement()
                {
                    LegalEntityId = legalEntityId
                },
                DraftVersion = submitActionRequest.Target_draft_version
            };

            var result = await SubmitForProcessing<ProductAgreement>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"Successfully submitted product agreement {productAgreementId} for legal entity {legalEntityId} of customer {customerId} with submitted version {result.SubmittedVersion}");
            return new ApiContract.SubmitActionResponse()
            {
                Entity_id = productAgreementId,
                Submitted_version = (long)result.SubmittedVersion
            };
        }

        [HttpDelete("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}")]
        public async Task<StatusCodeResult> RemoveProductAgreement([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId)
        {
            logger.LogInformation($"Removing product agreement {productAgreementId} for legal entity {legalEntityId} of customer {customerId}");
            var envelop = new MessageEnvelop()
            {
                Change = ChangeType.Delete,
                Name = EntityName.ProductAgreement,
                CustomerId = customerId,
                EntityId = productAgreementId,
                Draft = new ProductAgreement()
                {
                    LegalEntityId = legalEntityId
                },
            };

            var result = await SubmitForProcessing<ProductAgreement>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"Successfully removed product agreement {productAgreementId} for legal entity {legalEntityId} of customer {customerId}");
            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/product-agreements")]
        public async Task<ActionResult<ApiContract.EntityResponse_ProductAgreement>> CreateProductAgreement([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromBody] ApiContract.CreateProductAgreement productAgreement)
        {
            logger.LogInformation($"Creating product agreement for legal entity {legalEntityId} of customer {customerId}");
            var domainProductAgreement = ApiContractProductAgreement_ToModelProductAgreementMap.Convert(productAgreement, legalEntityId);

            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.ProductAgreement,
                Draft = domainProductAgreement,
                CustomerId = customerId
            };

            var result = await SubmitForProcessing<ProductAgreement>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"Successfully created product agreement {result.EntityId} for legal entity {legalEntityId} of customer {customerId}");
            return MessageEnvelop_ToEntityResponse_ProductAgreement.Convert(result);
        }

        [HttpGet("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_ProductAgreement>> GetProductAgreementById(string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId)
        {
            logger.LogInformation($"Getting product agreement {productAgreementId} for legal entity {legalEntityId} of customer {customerId}");
            var entityDocument = await customerDatabase.FindEntity<ProductAgreement>(LookupPredicate.Create(productAgreementId, customerId, legalEntityId));
            if (entityDocument == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"Successfully got product agreement {productAgreementId} for legal entity {legalEntityId} of customer {customerId}");
            return MessageEnvelop_ToEntityResponse_ProductAgreement.Convert(entityDocument);
        }

        [HttpPatch("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_ProductAgreement>> UpdateProductAgreement([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId, [FromBody] ApiContract.UpdateProductAgreement patch)
        {
            logger.LogInformation($"Updating product agreement {productAgreementId} for legal entity {legalEntityId} of customer {customerId} with target draft version {patch.Target_draft_version}");
            var patchModel = ApiContractProductAgreement_ToModelProductAgreementMap.Update(patch, legalEntityId);

            var envelop = new MessageEnvelop
            {
                EntityId = productAgreementId,
                Change = ChangeType.Update,
                Name = EntityName.ProductAgreement,
                Draft = patchModel,
                CustomerId = customerId,
                DraftVersion = (decimal)patch.Target_draft_version
            };

            var result = await SubmitForProcessing<ProductAgreement>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            logger.LogInformation($"Successfully updated product agreement {productAgreementId} for legal entity {legalEntityId} of customer {customerId} with submitted version {result.SubmittedVersion}");
            return await GetProductAgreementById(customerId, legalEntityId, productAgreementId);
        }
    }
}

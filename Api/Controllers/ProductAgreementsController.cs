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
        public ProductAgreementsController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase) : base(changeProcessor, customerDatabase)
        {
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}/touch")]
        public async Task<ActionResult<EntityDocumentModel>> TouchProductAgreement([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId)
        {
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

            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}/submit")]
        public async Task<ActionResult<ApiContract.SubmitActionResponse>> SubmitProductAgreement([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId, [FromBody] ApiContract.SubmitActionRequest submitActionRequest)
        {
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
                DraftVersion = submitActionRequest.Draft_version
            };

            var result = await SubmitForProcessing<ProductAgreement>(envelop);
            if (result == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return new ApiContract.SubmitActionResponse()
            {
                Entity_id = legalEntityId,
                Submitted_version = (long)result.SubmittedVersion
            };
        }

        [HttpDelete("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}")]
        public async Task<ActionResult<EntityDocumentModel>> RemoveProductAgreement([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId)
        {
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

            return new NoContentResult();
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/product-agreements")]
        public async Task<ActionResult<ApiContract.EntityResponse_ProductAgreement>> CreateProductAgreement([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromBody] ApiContract.CreateUpdateProductAgreement productAgreement)
        {
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

            return MessageEnvelop_ToEntityResponse_ProductAgreement.Convert(result);
        }

        [HttpGet("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}")]
        public async Task<ActionResult<ApiContract.EntityResponse_ProductAgreement>> GetProductAgreementById(string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId)
        {
            var entityDocument = await customerDatabase.FindEntity<ProductAgreement>(LookupPredicate.Create(productAgreementId, customerId, legalEntityId));
            if (entityDocument == MessageEnvelop.NONE)
            {
                return NotFound();
            }

            return MessageEnvelop_ToEntityResponse_ProductAgreement.Convert(entityDocument);
        }

        [HttpPatch("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}")]
        public async Task<ActionResult<EntityDocumentModel>> UpdateProductAgreement([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId, [FromBody] ProductAgreementModel patch)
        {
            var patchModel = ProductAgreementToPatch(patch, legalEntityId);

            var envelop = new MessageEnvelop
            {
                EntityId = productAgreementId,
                Change = ChangeType.Update,
                Name = EntityName.ProductAgreement,
                Draft = patchModel,
                CustomerId = customerId,
                DraftVersion = patch.TargetVersion
            };

            return await Process<ProductAgreement>(envelop);
        }

        private static ProductAgreement ProductAgreementToPatch(ProductAgreementModel patchModel, string legalEntityId)
        {
            var productAgreement = new ProductAgreement
            {
                LegalEntityId = legalEntityId,
                ProductType = patchModel.ProductType,
                DisplayName = patchModel.DisplayName,
                RateCardId = patchModel.RateCardId,
                Label = patchModel.Label
                
            };

            if (patchModel.Configuration != null)
            {
                productAgreement.Configuration = [.. patchModel.Configuration.Select(a => new ProductConfiguration() { Key = a.Key, Value = a.Value })];
            }

            if (patchModel.Features != null)
            {
                productAgreement.Features = [.. patchModel.Features.Select(a => new ProductFeature() { Key = a.Key, Value = a.Value })];
            }

            if (patchModel.Descriptors != null)
            {
                // productAgreement.MetaData = [.. patchModel.MetaData.Select(a => new MetaDataModel() { Key = a.Key, Value = a.Value })];
            }

            return productAgreement;
        }
    }
}

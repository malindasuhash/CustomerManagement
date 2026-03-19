using Api.ApiModels;
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

            return await Touch<ProductAgreement>(envelop);
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}/submit")]
        public async Task<ActionResult<EntityDocumentModel>> SubmitProductAgreement([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId, [FromBody] SubmitEntityModel submitModel)
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
                DraftVersion = submitModel.TargetVersion
            };

            return await Submit<ProductAgreement>(envelop);
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

            return await Remove<ProductAgreement>(envelop);
        }

        [HttpPost("{customerId}/legal-entities/{legalEntityId}/product-agreements")]
        public async Task<ActionResult<EntityDocumentModel>> CreateProductAgreement([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromBody] ProductAgreement productAgreement)
        {
            productAgreement.LegalEntityId = legalEntityId; // Legal entity scoped.

            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.ProductAgreement,
                Draft = productAgreement,
                CustomerId = customerId
            };

            return await Create<ProductAgreement>(envelop);
        }

        [HttpGet("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}")]
        public async Task<ActionResult<EntityDocumentModel>> GetBankAccountById(string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId)
        {
            return await GetById<ProductAgreement>(LookupPredicate.Create(productAgreementId, customerId, legalEntityId));
        }

        [HttpPatch("{customerId}/legal-entities/{legalEntityId}/product-agreements/{productAgreementId}")]
        public async Task<ActionResult<EntityDocumentModel>> UpateContact([FromRoute] string customerId, [FromRoute] string legalEntityId, [FromRoute] string productAgreementId, [FromBody] ProductAgreementModel patch)
        {
            var patchModel = ContactToPatch(patch);

            var envelop = new MessageEnvelop
            {
                EntityId = productAgreementId,
                Change = ChangeType.Update,
                Name = EntityName.ProductAgreement,
                Draft = patchModel,
                CustomerId = customerId,
                DraftVersion = patch.TargetVersion
            };

            await changeProcessor.ProcessChangeAsync<ProductAgreement>(envelop);

            var contactEntity = await customerDatabase.FindEntity<ProductAgreement>(envelop.SearchBy());

            return Translate(contactEntity);
        }

        private static ProductAgreement ContactToPatch(ProductAgreementModel patchModel)
        {
            var productAgreement = new ProductAgreement
            {
                LegalEntityId = patchModel.LegalEntityId,
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
                productAgreement.Descriptors = [.. patchModel.Descriptors.Select(a => new Descriptor() { Key = a.Key, Value = a.Value })];
            }

            return productAgreement;
        }
    }
}

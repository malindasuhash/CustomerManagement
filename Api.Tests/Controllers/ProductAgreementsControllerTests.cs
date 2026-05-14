using Api.ApiModels;
using Api.Controllers;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Tests.Controllers
{
    public class ProductAgreementsControllerTests
    {
        const string LegalEntityId = "LegalEntityId1";
        const string CustomerId = "CustomerId1";
        const string ProductAgreementId = "ProductAgreementId1";

        private readonly ProductAgreementsController productAgreementController;
        private readonly IChangeProcessor changeProcessor;
        private readonly ICustomerDatabase customerDatabase;


        public ProductAgreementsControllerTests()
        {
            changeProcessor = Substitute.For<IChangeProcessor>();
            customerDatabase = Substitute.For<ICustomerDatabase>();
            customerDatabase.FindEntity<ProductAgreement>(Arg.Any<LookupPredicate>()).Returns(new MessageEnvelop() { CustomerId = CustomerId });
            productAgreementController = new ProductAgreementsController(changeProcessor, customerDatabase);
        }

        [Fact]
        public async Task TouchProductAgreement_WhenInvokedThen_ExpectedEnvelopIsCreated()
        {
            // Act
            await productAgreementController.TouchProductAgreement(CustomerId, LegalEntityId, ProductAgreementId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<ProductAgreement>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(ProductAgreementId) && p.Change == ChangeType.Touch && p.Name == EntityName.ProductAgreement && p.CustomerId.Equals(CustomerId) && LegalEntityIdCheck(p, LegalEntityId)));
        }


        [Fact]
        public async Task SubmitProductAgreement_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await productAgreementController.SubmitProductAgreement(CustomerId, LegalEntityId, ProductAgreementId, new ApiContract.SubmitActionRequest() { Target_draft_version = 10 });

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<ProductAgreement>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(ProductAgreementId) && p.Change == ChangeType.Submit && p.Name == EntityName.ProductAgreement && p.CustomerId.Equals(CustomerId) && LegalEntityIdCheck(p, LegalEntityId) && p.DraftVersion.Equals(10)));
        }


        [Fact]
        public async Task RemoveProductAgreement_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await productAgreementController.RemoveProductAgreement(CustomerId, LegalEntityId, ProductAgreementId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<ProductAgreement>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(ProductAgreementId) && p.Change == ChangeType.Delete && p.Name == EntityName.ProductAgreement && p.CustomerId.Equals(CustomerId) && LegalEntityIdCheck(p, LegalEntityId)));
        }

        [Fact]
        public async Task CreateProductAgreement_WhenInvoked_ThenUseAccurateCommand()
        {
            var productAgreement = new ApiContract.CreateProductAgreement();

            // Act
            await productAgreementController.CreateProductAgreement(CustomerId, LegalEntityId, productAgreement);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<ProductAgreement>(Arg.Is<MessageEnvelop>(p => p.CustomerId.Equals(CustomerId) && LegalEntityIdCheck(p, LegalEntityId)));
        }

        [Fact]
        public async Task GetProductAgreementById_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await productAgreementController.GetProductAgreementById(CustomerId, LegalEntityId, ProductAgreementId);

            // Assert
            await customerDatabase.Received(1).FindEntity<ProductAgreement>(Arg.Is<LookupPredicate>(p => p.CustomerId.Equals(CustomerId) && p.LegalEntityId.Equals(LegalEntityId) && p.EntityId.Equals(ProductAgreementId)));
        }

        [Fact]
        public async Task UpdateProductAgreement_WhenUpdating_ThenIssuesTheAppropriateCommand()
        {
            // Arrange
            var patchModel = new ApiContract.UpdateProductAgreement()
            {
                Display_name = "displayName1",
                // pro = "ProductType1",
                Labels = ["Label"],
                Target_draft_version = 10
            };

            // Act
            await productAgreementController.UpdateProductAgreement(CustomerId, LegalEntityId, ProductAgreementId, patchModel);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<ProductAgreement>(Arg.Is<MessageEnvelop>(m => SameAfterMapped(patchModel, m)));
        }

        private static bool SameAfterMapped(ApiContract.UpdateProductAgreement productAgreementModel, MessageEnvelop messageEnvelop)
        {
            var productAgreementMapped = messageEnvelop.Draft as ProductAgreement;

            return messageEnvelop.Name == EntityName.ProductAgreement
                && messageEnvelop.Change == ChangeType.Update
                && messageEnvelop.DraftVersion == 10
                && productAgreementMapped.DisplayName.Equals(productAgreementModel.Display_name)
                && productAgreementMapped.ProductType.Equals(productAgreementMapped.ProductType)
                && productAgreementMapped.Labels.Equals(productAgreementModel.Labels);
        }

        private static bool LegalEntityIdCheck(MessageEnvelop envelop, string legalEntityId)
        {
            var productAgreement = envelop.Draft as ProductAgreement;

            return productAgreement.LegalEntityId.Equals(legalEntityId);
        }
    }
}

using Api.ApiModels;
using Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Tests.Controllers
{
    public class LegalEntityControllerTests
    {

        const string LegalEntityId = "LegalEntityId1";
        const string CustomerId = "CustomerId1";

        private readonly LegalEntityController legalEntityController;
        private readonly IChangeProcessor changeProcessor;
        private readonly ICustomerDatabase customerDatabase;

        public LegalEntityControllerTests()
        {
            changeProcessor = Substitute.For<IChangeProcessor>();
            customerDatabase = Substitute.For<ICustomerDatabase>();
            customerDatabase.FindEntity<LegalEntity>(Arg.Any<LookupPredicate>()).Returns(new MessageEnvelop() { CustomerId = CustomerId });
            legalEntityController = new LegalEntityController(changeProcessor, customerDatabase, null, null, null);
        }

        [Fact]
        public async Task TouchLegalEntity_WhenInvokedThen_ExpectedEnvelopIsCreated()
        {
            // Act
            await legalEntityController.TouchLegalEntity(CustomerId, LegalEntityId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<LegalEntity>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(LegalEntityId) && p.Change == ChangeType.Touch && p.Name == EntityName.LegalEntity && p.CustomerId.Equals(CustomerId)));
        }

        [Fact]
        public async Task SubmitLegalEntity_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await legalEntityController.SubmitLegalEntity(CustomerId, LegalEntityId, new ApiContract.SubmitActionRequest() { Target_draft_version = 10 });

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<LegalEntity>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(LegalEntityId) && p.Change == ChangeType.Submit && p.Name == EntityName.LegalEntity && p.CustomerId.Equals(CustomerId) && p.DraftVersion.Equals(10)));
        }

        [Fact]
        public async Task RemoveLegalEntity_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await legalEntityController.RemoveLegalEntity(CustomerId, LegalEntityId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<LegalEntity>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(LegalEntityId) && p.Change == ChangeType.Delete && p.Name == EntityName.LegalEntity && p.CustomerId.Equals(CustomerId)));
        }

        [Fact]
        public async Task CreateCreateEntity_WhenInvoked_ThenUseAccurateCommand()
        {
            var legalEntity = new ApiContract.CreateLegalEntity();

            // Act
            await legalEntityController.CreateLegalEntity(CustomerId, legalEntity);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<LegalEntity>(Arg.Is<MessageEnvelop>(p => p.CustomerId.Equals(CustomerId)));
        }

        [Fact]
        public async Task GetLegalEntityById_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await legalEntityController.GetLegalEntityById(CustomerId, LegalEntityId);

            // Assert
            await customerDatabase.Received(1).FindEntity<LegalEntity>(Arg.Is<LookupPredicate>(p => p.CustomerId.Equals(CustomerId) && p.EntityId.Equals(LegalEntityId)));
        }

        [Fact]
        public async Task UpdateLegalEntity_WhenUpdating_ThenIssuesTheAppropriateCommand()
        {
            // Arrange
            var patchModel = new ApiContract.UpdateLegalEntity()
            {
                Business_email = "e@mail.com",
                Nature_of_business = "sole trader",
                Target_draft_version = 10
            };

            // Act
            await legalEntityController.UpdateLegalEntity(CustomerId, LegalEntityId, patchModel);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<LegalEntity>(Arg.Is<MessageEnvelop>(m => SameAfterMapped(patchModel, m)));
        }

        [Fact]
        public async Task FindLegalEntitiesByContact_WhenMissingParameters_ReturnsBadRequestAndDoesNotCallDatabase()
        {
            // Act
            var result = await legalEntityController.FindLegalEntitiesByContact(null, null);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("customerId and contactId are required query parameters.", bad.Value);
            await customerDatabase.DidNotReceiveWithAnyArgs().GetLegalEntitiesBy(default, default);
        }

        [Fact]
        public async Task FindLegalEntitiesByContact_WhenFound_ReturnsMappedEntityDocumentModels()
        {
            // Arrange
            const string contactId = "ContactId1";
            var envelopes = new List<MessageEnvelop>()
                {
                    new MessageEnvelop()
                    {
                        CustomerId = CustomerId,
                        EntityId = LegalEntityId,
                        DraftVersion = 2,
                        SubmittedVersion = 1,
                        AppliedVersion = 0,
                        Draft = new LegalEntity() { Name = "LE1" },
                        Submitted = new LegalEntity() { Name = "LE1-sub" },
                        Applied = null,
                        State = EntityState.NEW
                    }
                };

            customerDatabase.GetLegalEntitiesBy(CustomerId, contactId).Returns(envelopes);

            // Act
            var actionResult = await legalEntityController.FindLegalEntitiesByContact(CustomerId, contactId);

            // Assert DB call
            await customerDatabase.Received(1).GetLegalEntitiesBy(CustomerId, contactId);

            // Assert response
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var models = Assert.IsType<List<EntityDocumentModel>>(ok.Value);
            Assert.Single(models);
            var model = models.First();
            Assert.Equal(CustomerId, model.CustomerId);
            Assert.Equal(LegalEntityId, model.EntityId);
            Assert.Equal(2, model.DraftVersion);
            Assert.Equal(1, model.SubmittedVersion);
        }

        private static bool SameAfterMapped(ApiContract.UpdateLegalEntity legalEntityModel, MessageEnvelop messageEnvelop)
        {
            var legalEntityMapped = messageEnvelop.Draft as LegalEntity;

            return messageEnvelop.Name == EntityName.LegalEntity
                && messageEnvelop.Change == ChangeType.Update
                && messageEnvelop.DraftVersion == 10
                && legalEntityMapped.BusinessEmail.Equals(legalEntityModel.Business_email)
                && legalEntityMapped.BusinessType.Equals(legalEntityModel.Business_type);
        }
    }
}

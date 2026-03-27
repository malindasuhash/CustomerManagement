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
            legalEntityController = new LegalEntityController(changeProcessor, customerDatabase);
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
            await legalEntityController.SubmitLegalEntity(CustomerId, LegalEntityId, new SubmitEntityModel() { TargetVersion = 10 });

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
            var legalEntity = new LegalEntity();

            // Act
            await legalEntityController.CreateLegalEntity(CustomerId, legalEntity);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<LegalEntity>(Arg.Is<MessageEnvelop>(p => p.CustomerId.Equals(CustomerId) && SameDraft(legalEntity, p)));
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
            var patchModel = new LegalEntityModel()
            {
                BusinessEmail = "e@mail.com",
                BusinessType = "sole trader",
                TargetVersion = 10
            };

            // Act
            await legalEntityController.UpdateLegalEntity(CustomerId, LegalEntityId, patchModel);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<LegalEntity>(Arg.Is<MessageEnvelop>(m => SameAfterMapped(patchModel, m)));
        }

        private static bool SameAfterMapped(LegalEntityModel legalEntityModel, MessageEnvelop messageEnvelop)
        {
            var legalEntityMapped = messageEnvelop.Draft as LegalEntity;

            return messageEnvelop.Name == EntityName.LegalEntity
                && messageEnvelop.Change == ChangeType.Update
                && messageEnvelop.DraftVersion == 10
                && legalEntityMapped.BusinessEmail.Equals(legalEntityModel.BusinessEmail)
                && legalEntityMapped.BusinessType.Equals(legalEntityModel.BusinessType);
        }

        private static bool SameDraft(LegalEntity legalEntity, MessageEnvelop messageEnvelop)
        {
            return legalEntity == messageEnvelop.Draft;
        }
    }
}

using Api.ApiModels;
using Api.Controllers;
using Microsoft.AspNetCore.Mvc;
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
    public class TradingLocationControllerTests
    {
        const string LegalEntityId = "LegalEntityId1";
        const string CustomerId = "CustomerId1";
        const string TradingLocationId = "TradingLocationId1";

        private readonly TradingLocationController tradingLocationController;
        private readonly IChangeProcessor changeProcessor;
        private readonly ICustomerDatabase customerDatabase;


        public TradingLocationControllerTests()
        {
            changeProcessor = Substitute.For<IChangeProcessor>();
            customerDatabase = Substitute.For<ICustomerDatabase>();
            customerDatabase.FindEntity<TradingLocation>(Arg.Any<LookupPredicate>()).Returns(new MessageEnvelop() { CustomerId = CustomerId });
            tradingLocationController = new TradingLocationController(changeProcessor, customerDatabase);
        }

        [Fact]
        public async Task TouchTradingLocation_WhenInvokedThen_ExpectedEnvelopIsCreated()
        {
            // Act
            await tradingLocationController.TouchTradingLocation(CustomerId, LegalEntityId, TradingLocationId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<TradingLocation>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(TradingLocationId) && p.Change == ChangeType.Touch && p.Name == EntityName.TradingLocation && p.CustomerId.Equals(CustomerId) && LegalEntityIdCheck(p, LegalEntityId)));
        }

        [Fact]
        public async Task SubmitTradingLocation_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await tradingLocationController.SubmitTradingLocation(CustomerId, LegalEntityId, TradingLocationId, new ApiContract.SubmitActionRequest() { Target_draft_version = 10 });

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<TradingLocation>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(TradingLocationId) && p.Change == ChangeType.Submit && p.Name == EntityName.TradingLocation && p.CustomerId.Equals(CustomerId) && LegalEntityIdCheck(p, LegalEntityId) && p.DraftVersion.Equals(10)));
        }

        [Fact]
        public async Task RemoveTradingLocation_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await tradingLocationController.RemoveTradingLocation(CustomerId, LegalEntityId, TradingLocationId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<TradingLocation>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(TradingLocationId) && p.Change == ChangeType.Delete && p.Name == EntityName.TradingLocation && p.CustomerId.Equals(CustomerId) && LegalEntityIdCheck(p, LegalEntityId)));
        }

        [Fact]
        public async Task CreateTradingLocation_WhenInvoked_ThenUseAccurateCommand()
        {
            var tradingLocation = new ApiContract.CreateTradingLocation();

            // Act
            await tradingLocationController.CreateTradingLocation(CustomerId, LegalEntityId, tradingLocation);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<TradingLocation>(Arg.Is<MessageEnvelop>(p => p.CustomerId.Equals(CustomerId) && LegalEntityIdCheck(p, LegalEntityId)));
        }

        [Fact]
        public async Task GetTradingLocationById_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await tradingLocationController.GetTradingLocationById(CustomerId, LegalEntityId, TradingLocationId);

            // Assert
            await customerDatabase.Received(1).FindEntity<TradingLocation>(Arg.Is<LookupPredicate>(p => p.CustomerId.Equals(CustomerId) && p.LegalEntityId.Equals(LegalEntityId) && p.EntityId.Equals(TradingLocationId)));
        }

        [Fact]
        public async Task UpdateTradingLocation_WhenUpdating_ThenIssuesTheAppropriateCommand()
        {
            // Arrange
            var patchModel = new ApiContract.UpdateTradingLocation()
            {
                Name = "Name1",
                Website = "Website2",
                Target_draft_version = 10,
            };

            // Act
            await tradingLocationController.UpdateTradingLocation(CustomerId, LegalEntityId, TradingLocationId, patchModel);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<TradingLocation>(Arg.Is<MessageEnvelop>(m => SameAfterMapped(patchModel, m)));
        }

        private static bool SameAfterMapped(ApiContract.UpdateTradingLocation tradingLocationModel, MessageEnvelop messageEnvelop)
        {
            var tradingLocationMapped = messageEnvelop.Draft as TradingLocation;

            return messageEnvelop.Name == EntityName.TradingLocation
                && messageEnvelop.Change == ChangeType.Update
                && messageEnvelop.DraftVersion == 10
                && tradingLocationMapped.Name.Equals(tradingLocationModel.Name)
                && tradingLocationMapped.Website.Equals(tradingLocationModel.Website);
        }

        private static bool LegalEntityIdCheck(MessageEnvelop envelop, string legalEntityId)
        {
            var tradingLocation = envelop.Draft as TradingLocation;

            return tradingLocation.LegalEntityId.Equals(legalEntityId);
        }
    }
}

using ExternalAdapter.Interfaces;
using ExternalAdapter.Services.AmendContact;
using FluentAssertions;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalAdapter.Tests.Services.AmendContact
{
    public class TradingLocationContactUpdateCaseAssessementTests
    {


        private readonly IQuery query;
        private readonly IAsseement nextAssessement;
        private readonly TradingLocationContactUpdateCaseAssessement tradingLocationContactUpdateCaseAssessement;

        public TradingLocationContactUpdateCaseAssessementTests()
        {
            query = Substitute.For<IQuery>();
            nextAssessement = Substitute.For<IAsseement>();
            tradingLocationContactUpdateCaseAssessement = new TradingLocationContactUpdateCaseAssessement(query, nextAssessement);
        }

        [Fact]
        public async Task Assess_WhenTheContactIsTradingLocationManager_ThenMarksForCaseCreation()
        {
            // Arrange
            string CustomerId = "Customer1";
            string ContactId = "ContactId1";
            string LegalEntityId = "LegalEntityId1";
            string TradingLocationId = "TradingLocationId1";

            var tradingLocationSubmitted = new TradingLocation()
            {
                Contacts = [new ContactReference() { ContactId = ContactId, ContactType = ContactType.Manager }]
            };
            var tradingLocationApplied = new TradingLocation()
            {
                Contacts = [new ContactReference() { ContactId = ContactId, ContactType = ContactType.Manager }]
            };
            var envelop = new MessageEnvelop()
            {
                CustomerId = CustomerId,
                Submitted = tradingLocationSubmitted,
                Applied = tradingLocationApplied,
                EntityId = TradingLocationId
            };
            var orchestrationInfo = new OrchestrationInfo()
            {
                CustomerId = CustomerId,
                LegalEntityId = LegalEntityId,
                EntityId = ContactId,
                Applied = new Contact(),
                Submitted = new Contact(),
            };

            query.GetTradingLocationsByContactId(CustomerId, LegalEntityId, ContactId).Returns([envelop]);

            // Act
            await tradingLocationContactUpdateCaseAssessement.Assess(orchestrationInfo);

            // Assert
            tradingLocationContactUpdateCaseAssessement.CaseSummaries.Count.Should().Be(1);
            tradingLocationContactUpdateCaseAssessement.CaseSummaries[0].CaseType.Should().Be(CaseType.AmendContact);
            await nextAssessement.Received(1).Assess(orchestrationInfo);
        }
    }
}

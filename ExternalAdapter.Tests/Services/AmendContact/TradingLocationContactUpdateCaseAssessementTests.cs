using ExternalAdapter.Interfaces;
using ExternalAdapter.Services.AmendContact;
using FluentAssertions;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Tests.Services.AmendContact
{
    public class TradingLocationContactUpdateCaseAssessementTests
    {
        const string CustomerId = "Customer1";
        const string ContactId = "ContactId1";
        const string LegalEntityId = "LegalEntityId1";
        const string TradingLocationId = "TradingLocationId1";

        private readonly IQuery query;
        private readonly CaseAssessment nextAssessement;
        private readonly TradingLocationContactUpdateCaseAssessement tradingLocationContactUpdateCaseAssessement;

        public TradingLocationContactUpdateCaseAssessementTests()
        {
            query = Substitute.For<IQuery>();
            nextAssessement = Substitute.For<CaseAssessment>();
            tradingLocationContactUpdateCaseAssessement = new TradingLocationContactUpdateCaseAssessement(query, nextAssessement);
        }

        [Fact]
        public async Task Assess_WhenTheContactIsTradingLocationManager_ThenMarksForCaseCreation()
        {
            // Arrange
            var tradingLocationSubmitted = new TradingLocation()
            {
                Contacts = [new ContactReference() { ContactId = ContactId, ContactType = ContactType.Manager }]
            };
            var envelop = new MessageEnvelop()
            {
                CustomerId = CustomerId,
                Submitted = tradingLocationSubmitted,
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
            tradingLocationContactUpdateCaseAssessement.Case.Count.Should().Be(1);
            tradingLocationContactUpdateCaseAssessement.Case[0].CaseType.Should().Be(CaseType.AmendContactTradingLocation);
            tradingLocationContactUpdateCaseAssessement.Case[0].Status.Should().Be(CaseStatus.Candidate);
            tradingLocationContactUpdateCaseAssessement.Case[0].Identifiers["CustomerId"].Should().Be(CustomerId);
            tradingLocationContactUpdateCaseAssessement.Case[0].Identifiers["ContactId"].Should().Be(ContactId);
            await nextAssessement.Received(1).Assess(orchestrationInfo);
        }

        [Fact]
        public async Task Assess_WhenNoTradingLocationsFound_ThenNoCaseAddedAndNextAssessmentInvoked()
        {
            // Arrange
            var orchestrationInfo = new OrchestrationInfo()
            {
                CustomerId = CustomerId,
                LegalEntityId = LegalEntityId,
                EntityId = ContactId,
                Applied = new Contact(),
                Submitted = new Contact(),
            };

            query.GetTradingLocationsByContactId(CustomerId, LegalEntityId, ContactId).Returns([]);

            // Act
            await tradingLocationContactUpdateCaseAssessement.Assess(orchestrationInfo);

            // Assert
            tradingLocationContactUpdateCaseAssessement.Case.Should().BeEmpty();
            await nextAssessement.Received(1).Assess(orchestrationInfo);
        }

        [Fact]
        public async Task Assess_WhenContactLinkedWithNonManagerType_ThenNoCaseAddedAndNextAssessmentInvoked()
        {
            // Arrange
            var tradingLocationSubmitted = new TradingLocation()
            {
                Contacts = [new ContactReference() { ContactId = ContactId, ContactType = ContactType.Financial }]
            };
            var envelop = new MessageEnvelop()
            {
                CustomerId = CustomerId,
                Submitted = tradingLocationSubmitted,
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
            tradingLocationContactUpdateCaseAssessement.Case.Should().BeEmpty();
            await nextAssessement.Received(1).Assess(orchestrationInfo);
        }

        [Fact]
        public async Task Assess_WhenContactLinkedToMultipleTradingLocationsWithManagerType_ThenMultipleCasesAdded()
        {
            // Arrange
            var tradingLocationSubmitted1 = new TradingLocation()
            {
                Contacts = [new ContactReference() { ContactId = ContactId, ContactType = ContactType.Manager }]
            };
            var tradingLocationSubmitted2 = new TradingLocation()
            {
                Contacts = [new ContactReference() { ContactId = ContactId, ContactType = ContactType.Manager }]
            };
            var envelops = new List<MessageEnvelop>
                {
                    new MessageEnvelop()
                    {
                        CustomerId = CustomerId,
                        Submitted = tradingLocationSubmitted1,
                        EntityId = "TradingLocation1"
                    },
                    new MessageEnvelop()
                    {
                        CustomerId = CustomerId,
                        Submitted = tradingLocationSubmitted2,
                        EntityId = "TradingLocation2"
                    }
                };
            var orchestrationInfo = new OrchestrationInfo()
            {
                CustomerId = CustomerId,
                LegalEntityId = LegalEntityId,
                EntityId = ContactId,
                Applied = new Contact(),
                Submitted = new Contact(),
            };

            query.GetTradingLocationsByContactId(CustomerId, LegalEntityId, ContactId).Returns(envelops);

            // Act
            await tradingLocationContactUpdateCaseAssessement.Assess(orchestrationInfo);

            // Assert
            tradingLocationContactUpdateCaseAssessement.Case.Count.Should().Be(2);
            tradingLocationContactUpdateCaseAssessement.Case.Should().AllSatisfy(c =>
            {
                c.CaseType.Should().Be(CaseType.AmendContactTradingLocation);
                c.Status.Should().Be(CaseStatus.Candidate);
                c.Identifiers["CustomerId"].Should().Be(CustomerId);
                c.Identifiers["ContactId"].Should().Be(ContactId);
            });
            await nextAssessement.Received(1).Assess(orchestrationInfo);
        }
    }
}

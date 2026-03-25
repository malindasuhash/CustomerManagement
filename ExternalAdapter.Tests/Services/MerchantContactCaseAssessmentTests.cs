using ExternalAdapter.Services;
using FluentAssertions;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Tests.Services
{
    public class MerchantContactCaseAssessmentTests
    {
        const string CustomerId = "CustomerId1";
        const string ContactId1 = "1";

        private readonly IQuery query;
        private readonly MerchantContactCaseAssessment merchantContactCaseAssessment;
        private readonly IAsseement asseement;

        public MerchantContactCaseAssessmentTests()
        {
            query = Substitute.For<IQuery>();
            asseement = Substitute.For<IAsseement>();
            merchantContactCaseAssessment = new MerchantContactCaseAssessment(query, asseement);
        }

        [Fact]
        public async Task Assess_WhenContactHasChangedAndLinkedToAccount_ThenReturnsSummaryAndInvokeNextAssessment()
        {
            // Arrange
            var contactApplied = new Contact() { FirstName = "A", LastName = "B" };
            var contactSubmitted = new Contact() { FirstName = "P", LastName = "Q" };

            var orchestrationInfo = new OrchestrationInfo()
            {
                CustomerId = CustomerId,
                EntityId = ContactId1,
                Applied = contactApplied,
                Submitted = contactSubmitted
            };
            var legalEntities = new List<MessageEnvelop>()
            {
                new MessageEnvelop()
                { CustomerId = CustomerId,
                    Applied = new LegalEntity()
                        {
                           BusinessContacts =
                           [
                               new() { ContactType = ContactType.Account, ContactId = ContactId1 }
                           ]
                        },
                    Submitted = new LegalEntity()
                        {
                           BusinessContacts =
                           [
                               new() { ContactType = ContactType.Account, ContactId = ContactId1 }
                           ]
                        }
                }
            };

            query.GetLegalEntitiesByContactId(CustomerId, ContactId1).Returns(legalEntities);

            // Act
            await merchantContactCaseAssessment.Assess(orchestrationInfo);

            // Assert
            merchantContactCaseAssessment.CaseSummaries.Count().Should().Be(1);
            merchantContactCaseAssessment.CaseSummaries.First().CaseType.Should().Be(CaseType.AmendContact);
            await asseement.Received(1).Assess(orchestrationInfo);
        }
    }
}

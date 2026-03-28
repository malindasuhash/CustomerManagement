using ExternalAdapter.Interfaces;
using ExternalAdapter.Services.AmendContact;
using FluentAssertions;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Tests.Services.AmendContact
{
    public class BillingContactUpdateCaseAssessmentTests
    {
        const string CustomerId = "CustomerId1";
        const string ContactId1 = "1";

        private readonly IQuery query;
        private readonly BillingContactUpdateCaseAssessment billingContactUpdateCaseAssessment;
        private readonly CaseAssessment asseement;

        public BillingContactUpdateCaseAssessmentTests()
        {
            query = Substitute.For<IQuery>();
            asseement = Substitute.For<CaseAssessment>();
            billingContactUpdateCaseAssessment = new BillingContactUpdateCaseAssessment(query, asseement);
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
                new()
                { 
                    CustomerId = CustomerId,
                    Applied = new LegalEntity()
                        {
                           BusinessContacts =
                           [
                               new() { ContactType = ContactType.Financial, ContactId = ContactId1 }
                           ]
                        },
                    Submitted = new LegalEntity()
                        {
                           BusinessContacts =
                           [
                               new() { ContactType = ContactType.Financial, ContactId = ContactId1 }
                           ]
                        }
                }
            };

            query.GetLegalEntitiesByContactId(CustomerId, ContactId1).Returns(legalEntities);

            // Act
            await billingContactUpdateCaseAssessment.Assess(orchestrationInfo);

            // Assert
            billingContactUpdateCaseAssessment.Case.Count().Should().Be(1);
            billingContactUpdateCaseAssessment.Case.First().CaseType.Should().Be(CaseType.AmendContact);
            await asseement.Received(1).Assess(orchestrationInfo);
        }
    }
}

using ExternalAdapter.Services;
using FluentAssertions;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalAdapter.Tests.Services
{
    public class MerchantContactUpdateAssessmentTests
    {
        const string CustomerId = "CustomerId1";
        const string ContactId1 = "1";
        const string ContactId2 = "2";

        private readonly IQuery query;
        private readonly List<CaseSummary> caseSummaries;
        private readonly MerchantContactUpdateAssessment merchantContactUpdateAssessment;
        public MerchantContactUpdateAssessmentTests()
        {
            query = Substitute.For<IQuery>();
            merchantContactUpdateAssessment = new MerchantContactUpdateAssessment(query, caseSummaries);
        }

        [Fact]
        public void GetSummary_WhenContactHasChangedAndLinkedToAccount_ThenReturnsSummary()
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
            merchantContactUpdateAssessment.Assess(orchestrationInfo);

            // Assert
            caseSummaries.First().CaseType.Should().Be(CaseType.AmendContact);
        }

        [Fact]
        public void GetSummary_WhenThereIsNoAppliedState_ThenMarksAsOnboarding()
        {
            // Arrange
            var orchestrationInfo = new OrchestrationInfo()
            {
                Applied = null,
                Submitted = new Contact()
            };

            // Act
            merchantContactUpdateAssessment.Assess(orchestrationInfo);

            // Assert
            caseSummaries.First().Should().Be(CaseSummary.ONBOARDING);
        }
    }
}

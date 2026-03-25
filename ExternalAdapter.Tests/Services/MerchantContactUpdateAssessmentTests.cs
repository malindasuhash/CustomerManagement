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
        private readonly MerchantContactCaseAssessment merchantContactCaseAssessment;
        private readonly IAsseement asseement;

        public MerchantContactUpdateAssessmentTests()
        {
            query = Substitute.For<IQuery>();
            asseement = Substitute.For<IAsseement>();
            merchantContactCaseAssessment = new MerchantContactCaseAssessment(query, asseement);
        }

        [Fact]
        public void Assess_WhenContactHasChangedAndLinkedToAccount_ThenReturnsSummary()
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
            merchantContactCaseAssessment.Assess(orchestrationInfo);

            // Assert
            merchantContactCaseAssessment.CaseSummaries.First().CaseType.Should().Be(CaseType.AmendContact);
        }
    }
}

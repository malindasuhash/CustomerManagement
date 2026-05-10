using ExternalAdapter.Interfaces;
using ExternalAdapter.Services.AmendContact;
using FluentAssertions;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Tests.Services.AmendContact
{
    public class MerchantContactCaseAssessmentTests
    {
        const string CustomerId = "CustomerId1";
        const string ContactId1 = "1";

        private readonly IQueryApi query;
        private readonly MerchantContactCaseAssessment merchantContactCaseAssessment;
        private readonly CaseAssessment asseement;

        public MerchantContactCaseAssessmentTests()
        {
            query = Substitute.For<IQueryApi>();
            asseement = Substitute.For<CaseAssessment>();
            merchantContactCaseAssessment = new MerchantContactCaseAssessment(query, asseement);
        }

        [Fact]
        public async Task Assess_WhenContactHasChangedAndLinkedToAccount_ThenReturnsSummaryAndInvokeNextAssessment()
        {
            // Arrange
            var contactApplied = new Contact() {Name = "A" };
            var contactSubmitted = new Contact() {Name = "P" };

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
            merchantContactCaseAssessment.Case.Count().Should().Be(1);
            merchantContactCaseAssessment.Case.First().CaseType.Should().Be(CaseType.AmendContactMerchant);
            await asseement.Received(1).Assess(orchestrationInfo);
        }

        [Fact]
        public async Task Assess_WhenNoLegalEntitiesFound_ThenNoCaseAddedAndNextAssessmentInvoked()
        {
            // Arrange
            var contactApplied = new Contact() {Name = "A" };
            var contactSubmitted = new Contact() {Name = "P" };

            var orchestrationInfo = new OrchestrationInfo()
            {
                CustomerId = CustomerId,
                EntityId = ContactId1,
                Applied = contactApplied,
                Submitted = contactSubmitted
            };

            query.GetLegalEntitiesByContactId(CustomerId, ContactId1).Returns(new List<MessageEnvelop>());

            // Act
            await merchantContactCaseAssessment.Assess(orchestrationInfo);

            // Assert
            merchantContactCaseAssessment.Case.Should().BeEmpty();
            await asseement.Received(1).Assess(orchestrationInfo);
        }

        [Fact]
        public async Task Assess_WhenContactLinkedWithNonAccountType_ThenNoCaseAddedAndNextAssessmentInvoked()
        {
            // Arrange
            var contactApplied = new Contact() {Name = "A" };
            var contactSubmitted = new Contact() {Name = "P" };

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
            await merchantContactCaseAssessment.Assess(orchestrationInfo);

            // Assert
            merchantContactCaseAssessment.Case.Should().BeEmpty();
            await asseement.Received(1).Assess(orchestrationInfo);
        }

        [Fact]
        public async Task Assess_WhenContactLinkedToMultipleLegalEntitiesWithAccountType_ThenMultipleCasesAdded()
        {
            // Arrange
            var contactApplied = new Contact() {Name = "A" };
            var contactSubmitted = new Contact() {Name = "P" };

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
                    {
                        CustomerId = CustomerId,
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
                    },
                    new MessageEnvelop()
                    {
                        CustomerId = CustomerId,
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
            merchantContactCaseAssessment.Case.Count().Should().Be(2);
            merchantContactCaseAssessment.Case.Should().AllSatisfy(c =>
            {
                c.CaseType.Should().Be(CaseType.AmendContactMerchant);
                c.Status.Should().Be(CaseStatus.Candidate);
                c.Identifiers["CustomerId"].Should().Be(CustomerId);
                c.Identifiers["EntityId"].Should().Be(ContactId1);
            });
            await asseement.Received(1).Assess(orchestrationInfo);
        }
    }
}

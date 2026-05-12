using Api.ApiModels;
using Api.Controllers;
using NSubstitute;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Tests.Controllers
{
    public class BillingGroupControllerTests
    {
        const string CustomerId = "CustomerId1";
        const string BillingGroupId = "BillingGroupId1";

        private readonly BillingGroupController billingGroupController;
        private readonly IChangeProcessor changeProcessor;
        private readonly ICustomerDatabase customerDatabase;

        public BillingGroupControllerTests()
        {
            changeProcessor = Substitute.For<IChangeProcessor>();
            customerDatabase = Substitute.For<ICustomerDatabase>();
            customerDatabase.FindEntity<BankAccount>(Arg.Any<LookupPredicate>()).Returns(new MessageEnvelop() { CustomerId = CustomerId });
            billingGroupController = new BillingGroupController(changeProcessor, customerDatabase);
        }

        [Fact]
        public async Task TouchBillingGroup_WhenInvokedThen_ExpectedEnvelopIsCreated()
        {
            // Act
            await billingGroupController.TouchBillingGroup(CustomerId, BillingGroupId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<BillingGroup>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(BillingGroupId) && p.Change == ChangeType.Touch && p.Name == EntityName.BillingGroup && p.CustomerId.Equals(CustomerId)));
        }

        [Fact]
        public async Task SubmitBankAccount_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await billingGroupController.SubmitBillingGroup(CustomerId, BillingGroupId, new ApiContract.SubmitActionRequest() { Target_draft_version = 10 });

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<BillingGroup>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(BillingGroupId) && p.Change == ChangeType.Submit && p.Name == EntityName.BillingGroup && p.CustomerId.Equals(CustomerId) && p.DraftVersion.Equals(10)));
        }

        [Fact]
        public async Task RemoveBillingGroup_WhenInvoked_ThenUseAccurateCommand()
        {
            // Act
            await billingGroupController.RemoveBillingGroup(CustomerId, BillingGroupId);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<BillingGroup>(Arg.Is<MessageEnvelop>(p => p.EntityId.Equals(BillingGroupId) && p.Change == ChangeType.Delete && p.Name == EntityName.BillingGroup && p.CustomerId.Equals(CustomerId)));
        }

        [Fact]
        public async Task CreateBillingGroup_WhenInvoked_ThenUseAccurateCommand()
        {
            var billingGroup = new ApiContract.CreateBillingGroup();

            // Act
            await billingGroupController.CreateBillingGroup(CustomerId, billingGroup);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<BillingGroup>(Arg.Is<MessageEnvelop>(p => p.Change == ChangeType.Create && p.Name == EntityName.BillingGroup && p.CustomerId.Equals(CustomerId)));
        }

        [Fact]
        public async Task GetBillingGroupById_WhenInvoked_ThenUseAccurateCommand()
        {
            // Arrange
            customerDatabase.FindEntity<BillingGroup>(Arg.Any<LookupPredicate>()).Returns(new MessageEnvelop() { CustomerId = CustomerId });

            // Act
            await billingGroupController.GetBillingGroupById(CustomerId, BillingGroupId);

            // Assert
            await customerDatabase.Received(1).FindEntity<BillingGroup>(Arg.Is<LookupPredicate>(p => p.CustomerId.Equals(CustomerId) && p.EntityId.Equals(BillingGroupId)));
        }

        [Fact]
        public async Task UpdateBillingGroup_WhenUpdating_ThenIssuesTheAppropriateCommand()
        {
            // Arrange
            var patchModel = new BillingGroupModel()
            {
                Description = "description",
                Labels = ["label"],
                TargetVersion = 20
            };

            // Act
            await billingGroupController.UpateBillingGroup(CustomerId, BillingGroupId, patchModel);

            // Assert
            await changeProcessor.Received(1).ProcessChangeAsync<BillingGroup>(Arg.Is<MessageEnvelop>(m => SameAfterMapped(patchModel, m)));
        }

        private static bool SameAfterMapped(BillingGroupModel billingGroupModel, MessageEnvelop messageEnvelop)
        {
            var billingGroupMapped = messageEnvelop.Draft as BillingGroup;

            return messageEnvelop.Name == EntityName.BillingGroup
                && messageEnvelop.Change == ChangeType.Update
                && messageEnvelop.DraftVersion == 20
                && billingGroupMapped.Description == billingGroupModel.Description
                && billingGroupMapped.Labels == billingGroupModel.Labels;
        }

        //private static bool SameDraft(BillingGroup billingGroup, MessageEnvelop messageEnvelop)
        //{
        //    return billingGroup == messageEnvelop.Draft;
        //}
    }
}

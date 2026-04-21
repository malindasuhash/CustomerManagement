using StateManagment.Entity;

namespace Api.ApiModels
{
    public class BillingGroupModel
    {
        public string BillingBankAccountId { get; set; }
        public string Description { get; set; }
        public string LegalEntityId { get; set; }
        public string Name { get; set; }
        public DescriptorModel[] Descriptors { get; set; } = [];
        public string[] Labels { get; set; }
        public int TargetVersion { get; set; }
    }
}

using StateManagment.Entity;

namespace Api.ApiModels
{
    public class ProductAgreementModel
    {
        public string DisplayName { get; set; }
        public string ProductType { get; set; }
        public string RateCardId { get; set; }
        public ProductFeatureModel[] Features { get; set; }
        public ProductConfiguration[] Configuration { get; set; }
        public string LegalEntityId { get; set; }
        public DescriptorModel[] Descriptors { get; set; }
        public string Label { get; set; }

        public int TargetVersion { get; set; } = 0;
    }
}

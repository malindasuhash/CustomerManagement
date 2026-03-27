namespace Api.ApiModels
{
    public class BankAccountModel
    {
        public string BankAccountHolderNames { get; set; }
        public string AccountNumber { get; set; }
        public string BankCity { get; set; }
        public string BankCountry { get; set; }
        public string BankName { get; set; }
        public bool BillingDefault { get; set; }
        public string Iban { get; set; }
        public string Name { get; set; }
        public string SortCode { get; set; }
        public string Swift { get; set; }
        public DescriptorModel[] Descriptors { get; set; } = [];
        public string Label { get; set; }
        public int TargetVersion { get; set; }
    }
}

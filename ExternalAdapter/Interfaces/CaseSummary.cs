namespace ExternalAdapter.Interfaces
{
    public class CaseSummary
    {
        public static CaseSummary NA = new();
        public static CaseSummary ONBOARDING = new();

        public CaseType CaseType { get; set; }
        public dynamic CaseNote { get; set; }
    }
}

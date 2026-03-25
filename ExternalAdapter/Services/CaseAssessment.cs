using StateManagment.Models;

namespace ExternalAdapter.Services
{
    public abstract class CaseAssessment
    {
        protected IAsseement next;
        public List<CaseSummary> CaseSummaries { get; set; } = [];

        public void Assess(IAsseement nextAssessement)
        {
            next = nextAssessement;
        }

        public abstract Task Assess(OrchestrationInfo runtimeInfo);
    }
}

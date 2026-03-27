using ExternalAdapter.Services;
using StateManagment.Models;

namespace ExternalAdapter.Interfaces
{
    public abstract class CaseAssessment
    {
        protected CaseAssessment next;
        public List<CaseSummary> CaseSummaries { get; set; } = [];

        protected CaseAssessment()
        {
            next = null;
        }

        public CaseAssessment(CaseAssessment nextAssessement)
        {
            next = nextAssessement;
        }

        public abstract Task Assess(OrchestrationInfo runtimeInfo);
    }
}

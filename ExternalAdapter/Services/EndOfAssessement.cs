using ExternalAdapter.Interfaces;
using StateManagment.Models;

namespace ExternalAdapter.Services
{
    public class EndOfAssessement : CaseAssessment
    {
        public EndOfAssessement() : base()
        {
        }

        public override Task Assess(OrchestrationInfo runtimeInfo)
        {
            return Task.CompletedTask;
        }
    }
}

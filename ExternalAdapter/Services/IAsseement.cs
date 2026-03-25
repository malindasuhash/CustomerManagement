using StateManagment.Models;

namespace ExternalAdapter.Services
{
    public interface IAsseement
    {
        Task Assess(OrchestrationInfo orchestrationInfo);
    }
}

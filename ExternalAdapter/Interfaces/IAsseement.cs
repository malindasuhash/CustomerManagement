using StateManagment.Models;

namespace ExternalAdapter.Interfaces
{
    public interface IAsseement
    {
        Task Assess(OrchestrationInfo orchestrationInfo);
    }
}

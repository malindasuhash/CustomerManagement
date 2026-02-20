using StateManager.Models;

namespace StateManagment.Models
{
    public interface IStateManager
    {
        Task<TaskOutcome> ProcessUpdateAsync(OrchestrationEnvelop orchestrationEnvelop);
    }
}
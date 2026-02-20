using StateManager.Models;

namespace StateManager
{
    public interface IStateManager
    {
        Task<TaskOutcome> ProcessUpdateAsync(OrchestrationEnvelop orchestrationEnvelop);
    }
}
using StateManagment.Entity;

namespace StateManagment.Models
{
    public interface IStateManager
    {
        Task<TaskOutcome> ProcessUpdateAsync<T>(OrchestrationEnvelop orchestrationEnvelop) where T : IEntity;

        Task<TaskOutcome> Evaluate<T>(MessageEnvelop envelop) where T : IEntity;
    }
}
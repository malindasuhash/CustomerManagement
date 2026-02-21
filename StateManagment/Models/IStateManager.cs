namespace StateManagment.Models
{
    public interface IStateManager
    {
        Task<TaskOutcome> ProcessUpdateAsync(OrchestrationEnvelop orchestrationEnvelop);

        Task<TaskOutcome> Initiate(EntityName name, string entityId);
    }
}
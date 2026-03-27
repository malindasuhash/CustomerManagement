using StateManagment.Entity;

namespace StateManagment.Models
{
    public interface IChangeProcessor
    {
        Task<TaskOutcome> ProcessChangeAsync<T>(MessageEnvelop envelop) where T : IEntity;
    }
}
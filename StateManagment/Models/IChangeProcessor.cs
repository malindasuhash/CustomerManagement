namespace StateManagment.Models
{
    public interface IChangeProcessor
    {
        Task<TaskOutcome> ProcessChangeAsync(MessageEnvelop envelop);
    }
}
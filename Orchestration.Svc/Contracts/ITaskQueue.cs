using Contact.Orchestration.Svc.Model;
using StateManagment.Entity;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Contracts
{
    public interface ITaskQueue
    {
        Task Enqueue(EntityName name, WorkItemType work, RequestData requestData);

        Task Dequeue(CancellationToken cancellationToken);
    }
}
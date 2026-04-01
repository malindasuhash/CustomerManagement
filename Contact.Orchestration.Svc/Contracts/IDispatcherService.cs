using Contact.Orchestration.Svc.Model;

namespace Contact.Orchestration.Svc.Contracts
{
    public interface IDispatcherService
    {
        Task Dispatch(WorkItem workItem, CancellationToken cancellationToken);
    }
}
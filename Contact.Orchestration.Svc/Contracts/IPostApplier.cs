using Contact.Orchestration.Svc.Model;

namespace Contact.Orchestration.Svc.Contracts
{
    public interface IPostApplier
    {
        Task PostApply(RequestData requestData, CancellationToken stoppingToken);
    }
}
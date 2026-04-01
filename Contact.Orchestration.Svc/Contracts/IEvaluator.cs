using Contact.Orchestration.Svc.Model;

namespace Contact.Orchestration.Svc.Contracts
{
    public interface IEvaluator
    {
        Task Evaluate(RequestData orchestrationInfo, CancellationToken stoppingToken);
    }
}
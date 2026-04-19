using Contact.Orchestration.Svc.Model;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Contracts
{
    public interface IEvaluator
    {
        public EntityName Name { get; }
        Task Evaluate(RequestData orchestrationInfo, CancellationToken stoppingToken);
    }
}
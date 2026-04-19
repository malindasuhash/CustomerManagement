using Contact.Orchestration.Svc.Model;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Contracts
{
    public interface IPostApplier
    {
        public EntityName Name { get; }
        Task PostApply(RequestData requestData, CancellationToken stoppingToken);
    }
}
using Contact.Orchestration.Svc.Model;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Contracts
{
    public interface IApplier
    {
        public EntityName Name { get; }
        Task Apply(RequestData orchestrationInfo, CancellationToken stoppingToken);
    }
}
using Contact.Orchestration.Svc.Model;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Contracts
{
    public interface IApplier
    {
        Task Apply(RequestData orchestrationInfo, CancellationToken stoppingToken);
    }
}
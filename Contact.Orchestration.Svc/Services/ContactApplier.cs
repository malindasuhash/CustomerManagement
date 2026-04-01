
using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Model;

namespace Contact.Orchestration.Svc.Services
{
    public class ContactApplier : IApplier
    {
        public Task Apply(RequestData orchestrationInfo, CancellationToken stoppingToken) 
        {
            // NOP
            return Task.CompletedTask;
        }
    }
}
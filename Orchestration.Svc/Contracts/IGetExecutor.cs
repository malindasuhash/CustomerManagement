using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Model;
using StateManagment.Models;

namespace Orchestration.Svc.Contracts
{
    public interface IGetExecutor
    {
        IApplier? GetApplier(EntityName name);
        IEvaluator? GetEvaluator(EntityName name);
        IPostApplier? GetPostApplier(EntityName name);
    }
}
using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Model;
using Orchestration.Svc.Contracts;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Services
{
    public class GetExecutor : IGetExecutor
    {
        private readonly IEnumerable<IEvaluator> evaluators;
        private readonly IEnumerable<IApplier> appliers;
        private readonly IEnumerable<IPostApplier> postAppliers;

        public GetExecutor(IEnumerable<IEvaluator> evaluators, IEnumerable<IApplier> appliers, IEnumerable<IPostApplier> postAppliers)
        {
            this.evaluators = evaluators;
            this.appliers = appliers;
            this.postAppliers = postAppliers;
        }
        public IApplier? GetApplier(EntityName name)
        {
           foreach (var applier in appliers)
            {
                if (applier.Name == name)
                {
                    return applier;
                }
            }
            
           return null;
        }

        public IEvaluator? GetEvaluator(EntityName name)
        {
            foreach (var evaluator in evaluators)
            {
                if (evaluator.Name == name)
                {
                    return evaluator;
                }
            }

            return null;
        }

        public IPostApplier? GetPostApplier(EntityName name)
        {
            foreach (var postApplier in postAppliers)
            {
                if (postApplier.Name == name)
                {
                    return postApplier;
                }
            }

            return null;
        }
    }
}
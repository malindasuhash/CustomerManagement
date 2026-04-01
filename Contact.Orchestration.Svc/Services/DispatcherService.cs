using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Model;

namespace Contact.Orchestration.Svc.Services
{
    public class DispatcherService : IDispatcherService
    {
        private readonly IApplier contactApplier;
        private readonly IEvaluator contactEvaluator;

        public DispatcherService(IApplier contactApplier, IEvaluator contactEvaluator)
        {
            this.contactApplier = contactApplier;
            this.contactEvaluator = contactEvaluator;
        }
        public Task Dispatch(WorkItem workItem, CancellationToken cancellationToken)
        {
            if (workItem.For == WorkItemType.Evaluation)
            {
                contactEvaluator.Evaluate(workItem.OrchestrationInfo, cancellationToken);
            }
            else if (workItem.For == WorkItemType.Apply)
            {
                contactApplier.Apply(workItem.OrchestrationInfo, cancellationToken);
            }
            return Task.CompletedTask;
        }
    }
}
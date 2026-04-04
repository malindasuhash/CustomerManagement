using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Model;

namespace Contact.Orchestration.Svc.Services
{
    public class DispatcherService : IDispatcherService
    {
        private readonly IApplier contactApplier;
        private readonly IEvaluator contactEvaluator;
        private readonly IPostApplier contactPostApplier;

        public DispatcherService(IApplier contactApplier, IEvaluator contactEvaluator, IPostApplier contactPostApplier)
        {
            this.contactApplier = contactApplier;
            this.contactEvaluator = contactEvaluator;
            this.contactPostApplier = contactPostApplier;
        }
        public Task Dispatch(WorkItem workItem, CancellationToken cancellationToken)
        {
            switch (workItem.For)
            {
                case WorkItemType.Evaluation:
                    contactEvaluator.Evaluate(workItem.RequestData, cancellationToken);
                    break;
                case WorkItemType.Apply:
                    contactApplier.Apply(workItem.RequestData, cancellationToken);
                    break;
                case WorkItemType.PostApply:
                    contactPostApplier.PostApply(workItem.RequestData, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown work item type: {workItem.For}");
            }
            
            return Task.CompletedTask;
        }
    }
}
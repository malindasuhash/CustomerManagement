using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Model;
using Orchestration.Svc.Contracts;

namespace Contact.Orchestration.Svc.Services
{
    public class DispatcherService : IDispatcherService
    {
        private readonly IGetExecutor executor;

        public DispatcherService(IGetExecutor executor)
        {
            this.executor = executor;
        }

        public Task Dispatch(WorkItem workItem, CancellationToken cancellationToken)
        {
            switch (workItem.For)
            {
                case WorkItemType.Evaluation:
                    var evaluator = executor.GetEvaluator(workItem.Name);
                    if (evaluator != null)
                    {
                        return evaluator.Evaluate(workItem.RequestData, cancellationToken);
                    }
                    break;
                case WorkItemType.Apply:
                    var applier = executor.GetApplier(workItem.Name);
                    if (applier != null)
                    {
                        return applier.Apply(workItem.RequestData, cancellationToken);
                    }
                    break;
                case WorkItemType.PostApply:
                    var postApplier = executor.GetPostApplier(workItem.Name);
                    if (postApplier != null)
                    {
                        return postApplier.PostApply(workItem.RequestData, cancellationToken);
                    }
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
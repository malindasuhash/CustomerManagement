
using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Model;
using System.Threading.Channels;

namespace Contact.Orchestration.Svc.Services
{
    public class InMemoryChannelQueue : ITaskQueue
    {
        private readonly Channel<WorkItem> channel;
        private readonly IDispatcherService dispatcherService;

        public InMemoryChannelQueue(IDispatcherService dispatcherService)
        {
            this.channel = Channel.CreateUnbounded<WorkItem>(new UnboundedChannelOptions() { AllowSynchronousContinuations = false });
            this.dispatcherService = dispatcherService;
        }

        public async Task Enqueue(WorkItemType work, RequestData orchestrationInfo)
        {
            var workitem = new WorkItem() { For = work, OrchestrationInfo = orchestrationInfo };
            await channel.Writer.WriteAsync(workitem);
        }

        public async Task Dequeue(CancellationToken cancellationToken)
        {
            var workItemToProcess = await channel.Reader.ReadAsync();

            await dispatcherService.Dispatch(workItemToProcess, cancellationToken);
        }
    }
}
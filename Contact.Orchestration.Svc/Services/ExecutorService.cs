
using StateManagment.Models;
using System.Threading.Channels;

namespace Contact.Orchestration.Svc.Services
{
    public class ExecutorService : BackgroundService
    {
        private readonly ContactEvaluator contactEvaluator;
        private readonly ContactApplier contactApplier;

        public ExecutorService(ContactEvaluator contactEvaluator, ContactApplier contactApplier)
        {
            this.contactEvaluator = contactEvaluator;
            this.contactApplier = contactApplier;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }

    public class InMemoryChannelQueue
    {
        private readonly string queueName;
        private readonly Channel<OrchestrationInfo> channel;

        public InMemoryChannelQueue(string queueName)
        {
            this.channel = Channel.CreateUnbounded<OrchestrationInfo>(new UnboundedChannelOptions() { AllowSynchronousContinuations = false });
            this.queueName = queueName;
        }

        public async Task Enqueue(OrchestrationInfo orchestrationInfo)
        {
            await channel.Writer.WriteAsync(orchestrationInfo);
        }

        public async ValueTask<OrchestrationInfo> Dequeue()
        {
            return await channel.Reader.ReadAsync();
        }
    }

    public class ContactEvaluator
    {
        public void Evaluate(OrchestrationInfo orchestrationInfo, CancellationToken stoppingToken)
        {

        }
    }

    public class ContactApplier
    {
        public void Apply(OrchestrationInfo orchestrationInfo, CancellationToken stoppingToken) { }
    }
}
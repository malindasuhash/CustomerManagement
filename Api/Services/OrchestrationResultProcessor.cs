
using StateManagment.Models;

namespace Api.Services
{
    public class OrchestrationResultProcessor : BackgroundService
    {
        private readonly IReceiver receiver;

        public OrchestrationResultProcessor(IReceiver receiver)
        {
            this.receiver = receiver;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            receiver.ReceiveAsync();
            receiver.StartAsync();

            return Task.Delay(Timeout.Infinite, cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}

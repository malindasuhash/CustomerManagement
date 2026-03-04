
using StateManagment.Models;

namespace Api.Services
{
    /// <summary>
    /// Background listner to process orchestration results.
    /// </summary>
    public class OrchestrationResultProcessor : BackgroundService
    {
        private readonly IReceiver receiver;

        public OrchestrationResultProcessor(IReceiver receiver)
        {
            this.receiver = receiver;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            receiver.StartAsync();

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            receiver.StopAync();

            return Task.CompletedTask;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}

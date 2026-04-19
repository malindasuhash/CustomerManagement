using Contact.Orchestration.Svc.Contracts;

namespace Contact.Orchestration.Svc.Services
{
    public class ExecutorService : BackgroundService
    {
        private readonly ITaskQueue taskQueue;

        public ExecutorService(ITaskQueue taskQueue)
        {
            this.taskQueue = taskQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await taskQueue.Dequeue(stoppingToken);
            }
        }
    }
}
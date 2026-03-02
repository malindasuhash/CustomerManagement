using Azure.Messaging.ServiceBus;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class AzureServiceBusMessageReceiver : IReceiver
    {
        private readonly ServiceBusProcessor serviceBusProcessor;
        private readonly IStateManager stateManager;

        public AzureServiceBusMessageReceiver(IStateManager stateManager)
        {
            var connectionString = Environment.GetEnvironmentVariable("azureServiceBus.results.queue.listen");
            serviceBusProcessor = new ServiceBusClient(connectionString).CreateProcessor("cm.orchestration.results");
            this.stateManager = stateManager;
        }

        public void ReceiveAsync()
        {
            serviceBusProcessor.ProcessMessageAsync += ServiceBusProcessor_ProcessMessageAsync;
            
        }

        public void StartAsync()
        {
            serviceBusProcessor.StartProcessingAsync();
        }

        public void StopAync()
        {
            serviceBusProcessor.ProcessMessageAsync -= ServiceBusProcessor_ProcessMessageAsync;
            serviceBusProcessor.StopProcessingAsync();
        }

        private async Task ServiceBusProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var envelop = JsonSerializer.Deserialize<OrchestrationEnvelop>(arg.Message.Body);

            await stateManager.ProcessUpdateAsync(envelop);
        }
    }
}

using Azure.Messaging.ServiceBus;
using StateManagment.Entity;
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
            serviceBusProcessor = new ServiceBusClient(connectionString).CreateProcessor("cm.orchestration.results", new ServiceBusProcessorOptions 
            { 
                AutoCompleteMessages = true,
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
            });
            this.stateManager = stateManager;
        }

        public void StartAsync()
        {
            serviceBusProcessor.ProcessMessageAsync += ServiceBusProcessor_ProcessMessageAsync;
            serviceBusProcessor.ProcessErrorAsync += ServiceBusProcessor_ProcessErrorAsync;
            serviceBusProcessor.StartProcessingAsync();
        }

        private Task ServiceBusProcessor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            // NOP
            return Task.CompletedTask;
        }

        public void StopAync()
        {
            serviceBusProcessor.StopProcessingAsync();
            serviceBusProcessor.ProcessMessageAsync -= ServiceBusProcessor_ProcessMessageAsync;
            serviceBusProcessor.ProcessErrorAsync -= ServiceBusProcessor_ProcessErrorAsync;
        }

        private async Task ServiceBusProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var envelop = JsonSerializer.Deserialize<OrchestrationEnvelop>(arg.Message.Body);

            await stateManager.ProcessUpdateAsync<Contact>(envelop); // TODO
        }
    }
}

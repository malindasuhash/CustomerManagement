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
    public class AzureServiceBusMessageSender : ISender
    {
        private readonly ServiceBusSender serviceBusSender;

        public AzureServiceBusMessageSender()
        {
            var connectionString = Environment.GetEnvironmentVariable("azureServiceBus.results.queue");
            serviceBusSender = new ServiceBusClient(connectionString).CreateSender("cm.orchestration.results");
        }

        public async Task<TaskOutcome> SendAsync(object message, string correlationId)
        {
            var serviceBusMessage = new ServiceBusMessage()
            {
                CorrelationId = correlationId,
                ContentType = "application/json",
                Body = new BinaryData(JsonSerializer.Serialize(message))
            };

            await serviceBusSender.SendMessageAsync(serviceBusMessage);

            return TaskOutcome.OK;
        }
    }
}

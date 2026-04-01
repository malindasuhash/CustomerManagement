using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class AzureServiceBusMessageSender : ISender
    {
        private const string ENTITY = "CM_ENTITY";
        private const string STATE = "CM_STATE";
        private const string ENTITY_ID = "CM_ENTITY_ID";

        private static readonly string[] propertiesToIgnore =
          [
                nameof(MessageEnvelop.Name),
                nameof(MessageEnvelop.Change),
                nameof(MessageEnvelop.IsSubmitted),
                nameof(MessageEnvelop.OrchestrationData)
          ];

        private readonly JsonSerializerSettings settings;
        private readonly ServiceBusSender serviceBusSender;

        public AzureServiceBusMessageSender(string environmentVariable, string queueName)
        {
            var connectionString = Environment.GetEnvironmentVariable(environmentVariable);
            serviceBusSender = new ServiceBusClient(connectionString).CreateSender(queueName);
            settings = new JsonSerializerSettings
            {
                ContractResolver = new IgnorePropertiesResolver(propertiesToIgnore),
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() },
                Formatting = Formatting.Indented
            };
        }

        public async Task<TaskOutcome> SendAsync(OrchestrationEnvelop message, string correlationId)
        {
            var serviceBusMessage = new ServiceBusMessage()
            {
                CorrelationId = correlationId,
                ContentType = "application/json",
                Body = new BinaryData(JsonConvert.SerializeObject(message))
            };

            await serviceBusSender.SendMessageAsync(serviceBusMessage);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> DataChangedAsync(MessageEnvelop message, string correlationId)
        {
            var stringified = JsonConvert.SerializeObject(message, settings);

            var serviceBusMessage = new ServiceBusMessage()
            {
                CorrelationId = correlationId,
                ContentType = "application/json",
                Body = new BinaryData(stringified)
            };

            serviceBusMessage.ApplicationProperties.Add(ENTITY, message.Name.ToString());
            serviceBusMessage.ApplicationProperties.Add(ENTITY_ID, message.EntityId);
            serviceBusMessage.ApplicationProperties.Add(STATE, message.State.ToString());

            await serviceBusSender.SendMessageAsync(serviceBusMessage);

            return TaskOutcome.OK;
        }
    }
}

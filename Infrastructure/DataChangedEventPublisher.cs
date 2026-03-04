using Newtonsoft.Json;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class DataChangedEventPublisher : IEventPublisher
    {
        private readonly AzureServiceBusMessageSender sender;

        public DataChangedEventPublisher()
        {
            sender = new AzureServiceBusMessageSender("azureServiceBus.cm.contact.changefeed", "cm.contact.changefeed");
        }

        public async Task<TaskOutcome> DataChangedAsync(MessageEnvelop messageEnvelop)
        {
            return await sender.DataChangedAsync(messageEnvelop, Guid.NewGuid().ToString());
        }
    }
}

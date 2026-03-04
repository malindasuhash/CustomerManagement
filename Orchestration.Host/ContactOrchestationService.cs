using Infrastructure;
using StateManagment.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchestration.Host
{
    internal class ContactOrchestationService
    {
        public async Task Run()
        {
            var serviceBusQueue = new AzureServiceBusMessageSender("azureServiceBus.results.queue", "cm.orchestration.results");

            var contactOrchestration = new ContactOrchestration.Evaluate(serviceBusQueue);

            await contactOrchestration.Run(new ContactOrchestration.RuntimeInfo()
            {
                EntityId = "d9b15755-934b-4a57-a750-1359568e6a31",
                CorellationId = Guid.NewGuid().ToString(),
                AppliedVersion = 1,
                SubmittedVersion = 1,
                Submitted = new Contact() { FirstName = "9Malinda", LastName = "Suhash" }
            });
        }
    }
}

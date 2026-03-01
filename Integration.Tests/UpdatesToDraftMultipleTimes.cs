using Infrastructure;
using InMemory;
using StateManagment;
using StateManagment.Entity;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Tests
{
    internal class UpdatesToDraftMultipleTimes
    {
        public async Task UpdateDraft()
        {
            var database = new MongoCustomerDatabase();
            var distributedLock = new DictionaryLock();
            var eventPublisher = new SimpleEventPublisher();

            var changeHandler = new ChangeHandler(database, distributedLock, eventPublisher);

            var orchestrator = new BasicOrchestrator();
            var stateManager = new StateManager(changeHandler, orchestrator, database);

            var changeProcessor = new ChangeProcessor(changeHandler, stateManager);
            var contact = new Contact { FirstName = "John", LastName = "Doe" };
            // Create new Contact
            var envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Create,
                IsSubmitted = false,
                Draft = contact,
                CreatedUser = "Tester",
                UpdateUser = "Malinda"
            };

            await changeProcessor.ProcessChangeAsync(envelop);

            var entityId = envelop.EntityId;

            var contactDocument = await database.GetEntityDocument(EntityName.Contact, entityId);

            Console.WriteLine($"-> Created - Contact: {contactDocument}"); Console.WriteLine();

            contact.FirstName = "Malinda"; contact.LastName = null;

            envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Update,
                IsSubmitted = false,
                Draft = contact,
                CreatedUser = "Tester",
                UpdateUser = "Suhash",
                EntityId = entityId,
                DraftVersion = 1    
            };

            await changeProcessor.ProcessChangeAsync(envelop);

            contactDocument = await database.GetEntityDocument(EntityName.Contact, entityId);

            Console.WriteLine($"-> Updated 1 - Contact: {contactDocument}"); Console.WriteLine();

            contact.FirstName = "Amith"; contact.LastName = "K";

            envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Update,
                IsSubmitted = false,
                Draft = contact,
                CreatedUser = "Tester",
                UpdateUser = "Niranga",
                EntityId = entityId,
                DraftVersion = 2
            };

            await changeProcessor.ProcessChangeAsync(envelop);

            contactDocument = await database.GetEntityDocument(EntityName.Contact, entityId);

            Console.WriteLine($"-> Updated 2 - Contact: {contactDocument}"); Console.WriteLine();

            contact.FirstName = "Final"; contact.LastName = "Update";

            envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Update,
                IsSubmitted = true,
                Draft = contact,
                CreatedUser = "Tester",
                UpdateUser = "Malinda",
                EntityId = entityId,
                DraftVersion = 3
            };

            await changeProcessor.ProcessChangeAsync(envelop);

            Console.WriteLine($"-> Updated 3 (after submitting) - Contact: {contactDocument}"); Console.WriteLine();

            Console.ReadKey();
        }
    }
}

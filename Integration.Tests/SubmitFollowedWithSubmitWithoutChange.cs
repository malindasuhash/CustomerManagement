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
    internal class SubmitFollowedWithSubmitWithoutChange
    {
        public async Task SubmitNoChangeSubmit()
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

            envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Submit,
                IsSubmitted = false,
                UpdateUser = "Suhash",
                EntityId = entityId
            };

            await changeProcessor.ProcessChangeAsync(envelop);

            contactDocument = await database.GetEntityDocument(EntityName.Contact, entityId);

            Console.WriteLine($"-> Submitted 1 - Contact: {contactDocument}"); Console.WriteLine();

            envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Submit,
                IsSubmitted = false,
                UpdateUser = "Niranga",
                EntityId = entityId,
                DraftVersion = 2
            };

            var result = await changeProcessor.ProcessChangeAsync(envelop);

            contactDocument = await database.GetEntityDocument(EntityName.Contact, entityId);

            Console.WriteLine($"-> Submit without Update 2 - Contact: {contactDocument}"); Console.WriteLine();

            var outcome = result == TaskOutcome.NO_CHANGE_TO_SUBMIT ? "NO_CHANGE_TO_SUBMIT - Good" : "Unexpected";

            Console.WriteLine($"-> Result {outcome}"); Console.WriteLine();

            Console.ReadKey();
        }
    }
}

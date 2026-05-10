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
            var eventPublisher = new DataChangedEventPublisher();
            var auditManager = new AuditManager();

            var changeHandler = new ChangeHandler(database, database, eventPublisher, auditManager);

            var orchestrator = new BasicOrchestrator();
            var stateManager = new StateManager(changeHandler, orchestrator, database);

            var changeProcessor = new ChangeProcessor(changeHandler, stateManager);
            var contact = new Contact {Name = "John" };
            // Create new Contact
            var envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Create,
                IsSubmitted = false,
                Draft = contact,
                CreatedUser = "Tester",
                UpdateUser = "Malinda",
                CustomerId = "Cus123"
            };

            await changeProcessor.ProcessChangeAsync<Contact>(envelop);

            var entityId = envelop.EntityId;

            var contactDocument = await database.FindEntity<Contact>(envelop.SearchBy());

            Console.WriteLine($"-> Created - Contact: {contactDocument}"); Console.WriteLine();

            contact.Name = "Malinda";

            envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Update,
                IsSubmitted = false,
                Draft = contact,
                CreatedUser = "Tester",
                UpdateUser = "Suhash",
                EntityId = entityId,
                DraftVersion = 1,
                CustomerId = "Cus123"
            };

            await changeProcessor.ProcessChangeAsync<Contact>(envelop);

            contactDocument = await database.FindEntity<Contact>(envelop.SearchBy());

            Console.WriteLine($"-> Updated 1 - Contact: {contactDocument}"); Console.WriteLine();

            contact.Name = "Amith";

            envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Update,
                IsSubmitted = false,
                Draft = contact,
                CreatedUser = "Tester",
                UpdateUser = "Niranga",
                EntityId = entityId,
                DraftVersion = 2,
                CustomerId = "Cus123"
            };

            await changeProcessor.ProcessChangeAsync<Contact>(envelop);

            contactDocument = await database.FindEntity<Contact>(envelop.SearchBy());

            Console.WriteLine($"-> Updated 2 - Contact: {contactDocument}"); Console.WriteLine();

            contact.Name = "Final";

            envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Update,
                IsSubmitted = true,
                Draft = contact,
                CreatedUser = "Tester",
                UpdateUser = "Malinda",
                EntityId = entityId,
                DraftVersion = 3,
                CustomerId = "Cus123"
            };

            await changeProcessor.ProcessChangeAsync<Contact>(envelop);

            Console.WriteLine($"-> Updated 3 (after submitting) - Contact: {contactDocument}"); Console.WriteLine();

            Console.ReadKey();
        }
    }
}

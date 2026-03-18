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
    internal class TouchRerunEvaluation
    {
        public async Task Run()
        {
            var database = new MongoCustomerDatabase();
            var distributedLock = new DictionaryLock();
            var eventPublisher = new SimpleEventPublisher();
            var auditManager = new AuditManager();

            var changeHandler = new ChangeHandler(database, distributedLock, eventPublisher, auditManager);

            var orchestrator = new BasicOrchestrator();
            var stateManager = new StateManager(changeHandler, orchestrator, database);

            var changeProcessor = new ChangeProcessor(changeHandler, stateManager);

            // Create new Contact
            var envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Create,
                IsSubmitted = true,
                Draft = new Contact { FirstName = "John", LastName = "Doe" },
                CreatedUser = "Tester",
                UpdateUser = "Malinda",
                CustomerId = "Cus123"
            };

            await changeProcessor.ProcessChangeAsync<Contact>(envelop);

            var entityId = envelop.EntityId;

            var contactDocument = await database.GetEntityDocument(EntityName.Contact, envelop.EntityId);

            Console.WriteLine($"-> Before orchestration - Contact: {contactDocument}"); Console.WriteLine();

            Console.WriteLine($"--> Sent EVALUATION_STARTED"); Console.WriteLine();

            stateManager.ProcessUpdateAsync(StepToSend(contactDocument.EntityId, contactDocument.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED, [new Feedback() { Type = FeedbackType.Error, Key = "ALL_GOOD", Value = "BLA" }], [])).Wait();

            contactDocument = await database.GetEntityDocument(EntityName.Contact, contactDocument.EntityId);
            Console.WriteLine($" Contact: {contactDocument}"); Console.WriteLine();

            Console.WriteLine($"--> Sent EVALUATION_COMPLETED"); Console.WriteLine();

            stateManager.ProcessUpdateAsync(StepToSend(contactDocument.EntityId, contactDocument.SubmittedVersion, RuntimeStatus.EVALUATION_COMPLETED, [], [])).Wait();

            contactDocument = await database.GetEntityDocument(EntityName.Contact, contactDocument.EntityId);
            Console.WriteLine($"Contact: {contactDocument}"); Console.WriteLine();


            Console.WriteLine($"--> Sent CHANGE_APPLIED"); Console.WriteLine();

            stateManager.ProcessUpdateAsync(StepToSend(contactDocument.EntityId, contactDocument.SubmittedVersion, RuntimeStatus.CHANGE_APPLIED, [], [])).Wait();

            contactDocument = await database.GetEntityDocument(EntityName.Contact, contactDocument.EntityId);
            Console.WriteLine($"Contact: {contactDocument}"); Console.WriteLine();

            envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Touch,
                IsSubmitted = false,
                UpdateUser = "Malinda",
                EntityId = entityId,
                CustomerId = "Cus123"
            };

            await changeProcessor.ProcessChangeAsync<Contact>(envelop);

            contactDocument = await database.GetEntityDocument(EntityName.Contact, entityId);
            Console.WriteLine($"Contact after TOUCHED: {contactDocument}"); Console.WriteLine();

            Console.WriteLine($"--> Sent EVALUATION_STARTED after TOUCH"); Console.WriteLine();

            stateManager.ProcessUpdateAsync(StepToSend(contactDocument.EntityId, contactDocument.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED, [new Feedback() { Type = FeedbackType.Warning, Key = "ALL_GOOD_AFTER_TOUCH", Value = "OK" }], [])).Wait();

            contactDocument = await database.GetEntityDocument(EntityName.Contact, entityId);
            Console.WriteLine($"Contact after EVALUATION_STARTED: {contactDocument}"); Console.WriteLine();

            Console.ReadKey();
        }

        private static OrchestrationEnvelop StepToSend(string entityId, int submittedVersion, RuntimeStatus runtimeStatus, Feedback[] feedbacks, OrchestrationData[] orchestrationData)
        {
            var step = new OrchestrationEnvelop
            {
                EntityId = entityId,
                Name = EntityName.Contact,
                SubmittedVersion = submittedVersion,
                Status = runtimeStatus,
                Feedbacks = feedbacks,
                OrchestrationData = orchestrationData,
            };

            return step;
        }
    }
}

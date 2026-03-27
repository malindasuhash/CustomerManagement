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
    internal class CompleteFlowAndDelete
    {
        public async Task Delete()
        {
            var database = new MongoCustomerDatabase();
            var distributedLock = new DictionaryLock();
            var eventPublisher = new SimpleEventPublisher();
            var auditManager = new AuditManager();

            var changeHandler = new ChangeHandler(database, distributedLock, eventPublisher, auditManager);

            var orchestrator = new BasicOrchestrator();
            var stateManager = new StateManager(changeHandler, orchestrator, database);

            var changeProcessor = new ChangeProcessor(changeHandler, stateManager);
            var customerId = "customer1";

            // Create new Contact
            var envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Create,
                IsSubmitted = true,
                Draft = new Contact { FirstName = "John", LastName = "Doe" },
                CreatedUser = "Tester",
                UpdateUser = "Malinda",
                CustomerId = customerId
            };

            await changeProcessor.ProcessChangeAsync<Contact>(envelop);

            var entityId = envelop.EntityId;

            var contactDocument = await database.FindEntity<Contact>(envelop.SearchBy());

            Console.WriteLine($"-> Before orchestration - Contact: {contactDocument}"); Console.WriteLine();

            Console.WriteLine($"--> Sent EVALUATION_STARTED"); Console.WriteLine();

            stateManager.ProcessUpdateAsync<Contact>(StepToSend(contactDocument.EntityId, customerId, contactDocument.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED, [], [])).Wait();

            contactDocument = await database.FindEntity<Contact>(contactDocument.SearchBy());
            Console.WriteLine($" Contact: {contactDocument}"); Console.WriteLine();

            Console.WriteLine($"--> Sent EVALUATION_COMPLETED"); Console.WriteLine();

            stateManager.ProcessUpdateAsync<Contact>(StepToSend(contactDocument.EntityId, customerId, contactDocument.SubmittedVersion, RuntimeStatus.EVALUATION_COMPLETED, [], [])).Wait();

            contactDocument = await database.FindEntity<Contact>(contactDocument.SearchBy());
            Console.WriteLine($"Contact: {contactDocument}"); Console.WriteLine();


            Console.WriteLine($"--> Sent CHANGE_APPLIED"); Console.WriteLine();

            stateManager.ProcessUpdateAsync<Contact>(StepToSend(contactDocument.EntityId, customerId, contactDocument.SubmittedVersion, RuntimeStatus.CHANGE_APPLIED, [], [])).Wait();

            contactDocument = await database.FindEntity<Contact>(contactDocument.SearchBy());
            Console.WriteLine($"Contact: {contactDocument}"); Console.WriteLine();

            // Deleting 
            Console.WriteLine($"Starting to delete Entity Id: {entityId} ");

            envelop = new MessageEnvelop
            {
                Name = EntityName.Contact,
                Change = ChangeType.Delete,
                IsSubmitted = true,
                UpdateUser = "Bla",
                EntityId = entityId,
                CustomerId = "Cus123"
            };

            await changeProcessor.ProcessChangeAsync<Contact>(envelop);

            contactDocument = await database.FindEntity<Contact>(envelop.SearchBy());
            Console.WriteLine($"Contact after delete: {contactDocument}"); Console.WriteLine();

            Console.WriteLine($"--> Sent EVALUATION_STARTED after DELETE"); Console.WriteLine();

            stateManager.ProcessUpdateAsync<Contact>(StepToSend(entityId, customerId, contactDocument.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED, [new Feedback() { Type = FeedbackType.Warning, Key = "PROCESSING_DELETE", Value = "BLA" }], [])).Wait();

            Console.WriteLine($"--> Sent EVALUATION_COMPLETED after DELETE"); Console.WriteLine();

            stateManager.ProcessUpdateAsync<Contact>(StepToSend(entityId, customerId, contactDocument.SubmittedVersion, RuntimeStatus.EVALUATION_COMPLETED, [], [])).Wait();

            Console.WriteLine($"--> Sent CHANGE_APPLIED after DELETE"); Console.WriteLine();

            stateManager.ProcessUpdateAsync<Contact>(StepToSend(entityId, customerId, contactDocument.SubmittedVersion, RuntimeStatus.CHANGE_APPLIED, [], [])).Wait();

            contactDocument = await database.FindEntity<Contact>(contactDocument.SearchBy());
            Console.WriteLine($"Contact after DELETE: {contactDocument}"); Console.WriteLine();

            Console.ReadKey();
        }

        private static OrchestrationEnvelop StepToSend(string entityId, string customerId, int submittedVersion, RuntimeStatus runtimeStatus, Feedback[] feedbacks, OrchestrationData[] orchestrationData, string? legalEntityId = null)
        {
            var step = new OrchestrationEnvelop
            {
                EntityId = entityId,
                CustomerId = customerId,
                LegalEntityId = legalEntityId,
                Name = EntityName.Contact,
                SubmittedVersion = submittedVersion,
                Status = runtimeStatus,
                Feedbacks = feedbacks,
                OrchestrationData = orchestrationData
            };

            return step;
        }
    }
}

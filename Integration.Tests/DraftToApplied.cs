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
    internal class DraftToApplied
    {
        public async Task RunToApplied()
        {
            var database = new MongoCustomerDatabase();
            var distributedLock = new DictionaryLock();
            var eventPublisher = new DataChangedEventPublisher();
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

            var contactDocument = await database.FindEntity<Contact>(envelop.SearchBy());

            Console.WriteLine($"-> Before orchestration - Contact: {contactDocument}"); Console.WriteLine();

            Console.WriteLine($"--> Sent EVALUATION_STARTED"); Console.WriteLine();

            stateManager.ProcessUpdateAsync<Contact>(StepToSend(contactDocument.EntityId, customerId, contactDocument.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED, [], [])).Wait();

            contactDocument = await database.FindEntity<Contact>(contactDocument.SearchBy());
            Console.WriteLine($" Contact: {contactDocument}"); Console.WriteLine();

            Console.WriteLine($"--> Sent EVALUATION_COMPLETED"); Console.WriteLine();

            stateManager.ProcessUpdateAsync<Contact>(StepToSend(contactDocument.EntityId, customerId, contactDocument.SubmittedVersion, RuntimeStatus.EVALUATION_COMPLETED, [new Feedback() { Type = FeedbackType.Warning, Key = "FINE", Value = "S" }], [])).Wait();

            contactDocument = await database.FindEntity<Contact>(contactDocument.SearchBy());
            Console.WriteLine($"Contact: {contactDocument}"); Console.WriteLine();


            Console.WriteLine($"--> Sent CHANGE_APPLIED"); Console.WriteLine();

            stateManager.ProcessUpdateAsync<Contact>(StepToSend(contactDocument.EntityId, customerId, contactDocument.SubmittedVersion, RuntimeStatus.CHANGE_APPLIED, [], [new OrchestrationData() { Key = "AMAZING", Value = "WORLD"}])).Wait();

            contactDocument = await database.FindEntity<Contact>(contactDocument.SearchBy());
            Console.WriteLine($"Contact: {contactDocument}"); Console.WriteLine();

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

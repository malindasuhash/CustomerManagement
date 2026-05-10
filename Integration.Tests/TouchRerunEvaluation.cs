using Infrastructure;
using InMemory;
using StateManagment;
using StateManagment.Entity;
using StateManagment.Models;

namespace Integration.Tests
{
    internal class TouchRerunEvaluation
    {
        public async Task Run()
        {
            var database = new MongoCustomerDatabase();
            var eventPublisher = new DataChangedEventPublisher();
            var auditManager = new AuditManager();

            var changeHandler = new ChangeHandler(database, database, eventPublisher, auditManager);

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

            stateManager.ProcessUpdateAsync<Contact>(StepToSend(contactDocument.EntityId, customerId, contactDocument.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED, [new Feedback() {Type = FeedbackType.WaitingForExternalRiskChecks, Message = "BLA" }], [])).Wait();

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

            contactDocument = await database.FindEntity<Contact>(envelop.SearchBy());
            Console.WriteLine($"Contact after TOUCHED: {contactDocument}"); Console.WriteLine();

            Console.WriteLine($"--> Sent EVALUATION_STARTED after TOUCH"); Console.WriteLine();

            stateManager.ProcessUpdateAsync<Contact>(StepToSend(contactDocument.EntityId, customerId, contactDocument.SubmittedVersion, RuntimeStatus.EVALUATION_STARTED, [new Feedback() {Type = FeedbackType.WaitingForExternalRiskChecks, Message = "OK" }], [])).Wait();

            contactDocument = await database.FindEntity<Contact>(envelop.SearchBy());
            Console.WriteLine($"Contact after EVALUATION_STARTED: {contactDocument}"); Console.WriteLine();

            Console.ReadKey();
        }

        private static OrchestrationEnvelop StepToSend(string entityId, string customerId, int submittedVersion, RuntimeStatus runtimeStatus, Feedback[] feedbacks, OrchestrationData[] orchestrationData, string? legalEntityId = null)
        {
            var step = new OrchestrationEnvelop
            {
                EntityId = entityId,
                CustomerId= customerId,
                LegalEntityId = legalEntityId,
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

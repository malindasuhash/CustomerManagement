using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Controllers
{
    public abstract class EntityManagementController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase, ILogger logger) : ControllerBase
    {
        protected readonly IChangeProcessor changeProcessor = changeProcessor;
        protected readonly ICustomerDatabase customerDatabase = customerDatabase;
        private readonly ILogger logger = logger;

        protected async Task<MessageEnvelop> SubmitForProcessing<T>(MessageEnvelop messageEnvelop) where T : IEntity
        {
            var result = await changeProcessor.ProcessChangeAsync<T>(messageEnvelop);

            if (result != TaskOutcome.OK)
            {
                logger.LogError($"Failed to process change for entity {messageEnvelop.Name} with id {messageEnvelop.EntityId} for customer {messageEnvelop.CustomerId}. Result: {result}");
                return MessageEnvelop.NONE;
            }

            return await customerDatabase.FindEntity<T>(messageEnvelop.SearchBy());
        }
    }
}

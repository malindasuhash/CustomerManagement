using Microsoft.AspNetCore.Mvc;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Controllers
{
    public abstract class EntityManagementController(IChangeProcessor changeProcessor, ICustomerDatabase customerDatabase) : ControllerBase
    {
        protected readonly IChangeProcessor changeProcessor = changeProcessor;
        protected readonly ICustomerDatabase customerDatabase = customerDatabase;

        protected async Task<MessageEnvelop> SubmitForProcessing<T>(MessageEnvelop messageEnvelop) where T : IEntity
        {
            var result = await changeProcessor.ProcessChangeAsync<T>(messageEnvelop);

            if (result != TaskOutcome.OK)
            {
                return MessageEnvelop.NONE;
            }

            return await customerDatabase.FindEntity<T>(messageEnvelop.SearchBy());
        }
    }
}

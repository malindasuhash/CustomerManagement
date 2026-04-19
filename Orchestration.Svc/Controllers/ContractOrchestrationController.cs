using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Model;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/contact-orchestration")]
    public class ContractOrchestrationController : Controller
    {
        private readonly ITaskQueue taskQueue;

        public ContractOrchestrationController(ITaskQueue taskQueue)
        {
            this.taskQueue = taskQueue;
        }

        [HttpPost("evaluate")]
        public TaskOutcome Begin(ContactRequestData request)
        {
            taskQueue.Enqueue(EntityName.Contact, WorkItemType.Evaluation, request);

            return TaskOutcome.OK;
        }


        [HttpPost("apply")]
        public TaskOutcome Process(ContactRequestData request)
        {
            taskQueue.Enqueue(EntityName.Contact, WorkItemType.Apply, request);

            return TaskOutcome.OK;
        }

        [HttpPost("post-apply")]
        public TaskOutcome Finalise(ContactRequestData request)
        {
            taskQueue.Enqueue(EntityName.Contact, WorkItemType.PostApply, request);

            return TaskOutcome.OK;
        }

    }
}

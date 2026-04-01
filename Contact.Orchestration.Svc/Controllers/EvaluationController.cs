using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Model;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/contact-orchestration")]
    public class EvaluationController : Controller
    {
        private readonly ITaskQueue taskQueue;

        public EvaluationController(ITaskQueue taskQueue)
        {
            this.taskQueue = taskQueue;
        }

        [HttpPost("evaluate")]
        public EvaluationResponse Begin(ContactRequestData request)
        {
            taskQueue.Enqueue(WorkItemType.Evaluation, request);

            return new EvaluationResponse() { Result = "Accepted" };
        }
    }
}

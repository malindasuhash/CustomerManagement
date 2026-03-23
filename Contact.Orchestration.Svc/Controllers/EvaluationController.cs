using Microsoft.AspNetCore.Mvc;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/contact-orchestration")]
    public class EvaluationController : Controller
    {
        [HttpPost("/evaluate")]
        public IActionResult Start(OrchestrationInfo request)
        {
            return View();
        }
    }
}

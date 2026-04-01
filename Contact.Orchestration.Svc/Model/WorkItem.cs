using StateManagment.Entity;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Model
{
    public class WorkItem
    {
        public WorkItemType For { get; set; }
        public RequestData OrchestrationInfo { get; set; }
    }
}
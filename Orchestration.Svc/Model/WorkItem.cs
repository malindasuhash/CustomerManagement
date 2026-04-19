using StateManagment.Entity;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Model
{
    public class WorkItem
    {
        public EntityName Name { get; set; }
        public WorkItemType For { get; set; }
        public RequestData RequestData { get; set; }
    }
}
using StateManagment.Entity;
using StateManagment.Models;

namespace Contact.Orchestration.Svc.Model
{
    public class RequestData
    {
        public string Origin { get; set; }
        public string CorellationId { get; set; } = Guid.NewGuid().ToString();
        public string EntityId { get; set; }
        public string CustomerId { get; set; }
        public string LegalEntityId { get; set; }
        public int SubmittedVersion { get; set; }
        public int AppliedVersion { get; set; }
        public OrchestrationData[] OrchestrationData { get; set; } = [];
        public SystemDataModel[] SystemData { get; set; } = [];
    }
}

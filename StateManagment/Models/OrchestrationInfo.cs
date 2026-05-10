using StateManagment.Entity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StateManagment.Models
{
    /// <summary>
    /// This is the structure that us sent from CM to Orchestration
    /// to process a change.
    /// </summary>
    public class OrchestrationInfo
    {
        public string Origin { get; set; }
        public string CorellationId { get; set; } = Guid.NewGuid().ToString();
        public string EntityId { get; set; }
        public string CustomerId { get; set; }
        public string LegalEntityId { get; set; }
        public IEntity Submitted { get; set; }
        public IEntity Applied { get; set; }
        public int SubmittedVersion { get; set; }
        public int AppliedVersion { get; set; }
        public OrchestrationData[] OrchestrationData { get; set; } = [];
        public SystemDataModel[] SystemData { get; set; } = [];
    }
}

using StateManagment.Entity;
using StateManagment.Models;

namespace ContactOrchestration
{
    /// <summary>
    /// This is the structure that us sent from CM to Orchestration
    /// to process a change.
    /// </summary>
    public class RuntimeInfo
    {
        public string CorellationId { get; set; } = Guid.NewGuid().ToString();
        public string EntityId { get; set; }
        public string CustomerId { get; set; }
        public string LegalEntityId { get; set; }
        public IEntity Submitted { get; set; }
        public IEntity Applied { get; set; }
        public int SubmittedVersion { get; set; }
        public int AppliedVersion { get; set; }
        public OrchestrationData[] OrchestrationData { get; set; } = [];
        public SystemData[] SystemData { get; set; } = [];
    }
}

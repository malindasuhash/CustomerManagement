using StateManagment.Entity;

namespace ContactOrchestration
{
    public class RuntimeInfo
    {
        public string CorellationId { get; set; } = Guid.NewGuid().ToString();
        public string EntityId { get; set; }
        public IEntity Submitted { get; set; }
        public IEntity Applied { get; set; }
        public int SubmittedVersion { get; set; }
        public int AppliedVersion { get; set; }
        public string[] WorkflowData { get; set; } = new string[0];
    }
}

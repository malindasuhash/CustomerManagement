using StateManagment.Entity;

namespace ContactOrchestration
{
    public class RuntimeInfo
    {
        public string EntityId { get; set; }
        public IEntity Submitted { get; set; }
        public IEntity Applied { get; set; }
        public int SubmittedVersion { get; set; }
        public int AppliedVersion { get; set; }
    }
}

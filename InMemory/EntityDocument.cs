using StateManagment.Models;

namespace InMemory
{
    public class  EntityDocument
    {
        public string EntityId { get; set; }
        public dynamic Draft { get; set; }
        public int DraftVersion { get; set; }

        public dynamic Submitted { get; set; }
        public int SubmittedVersion { get; set; }

        public dynamic Applied { get; set; }
        public int AppliedVersion { get; set; }

        public string[] Messages { get; set; } = Array.Empty<string>();

        public EntityState State { get; set; }

        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}

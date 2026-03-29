using ExternalAdapter.Interfaces;
using StateManagment.Models;

namespace ExternalAdapter.Services.AmendContact
{
    /// <summary>
    /// Represents a customer data management case in an external system.
    /// </summary>
    public class ManagementCase
    {
        public Guid Id { get; set; }
        public string Origin { get; set; }
        public CaseType CaseType { get; set; }
        public CaseStatus Status { get; set; }
        public EntityName Name { get; set; }
        public List<EntityName> EntitiesToReevaluate { get; set; } = [];
        public dynamic Before { get; set; }
        public dynamic After { get; set; }
        public int SubmitedVersion { get; set; }    
        public int AppliedVersion { get; set; }
        public Dictionary<string, string> Identifiers { get; set; } = [];
        public string Checksum { get; set; }
        public DateTime? CreateDateTime { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}

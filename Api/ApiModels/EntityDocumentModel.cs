using StateManagment.Models;

namespace Api.ApiModels
{
    public class EntityDocumentModel
    {
        public string CustomerId { get; set; }
        public string EntityId { get; set; }
        
        public string State { get; set; }

        public dynamic Draft { get; set; }
        public int DraftVersion { get; set; }

        public dynamic Submitted { get; set; }
        public int SubmittedVersion { get; set; }

        public dynamic Applied { get; set; }
        public int AppliedVersion { get; set; }

        public string UpdateUser { get; set; }
        public DateTime? UpdateTimestamp { get; set; }

        public string CreatedUser { get; set; }
        public DateTime? CreatedTimestamp { get; set; }

        public Feedback[] Feedback { get; set; }

        public bool RemoveRequested { get; set; }

        public bool Removed { get; set; }
    }
}

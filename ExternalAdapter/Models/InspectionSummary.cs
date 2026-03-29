using ExternalAdapter.Services.AmendContact;

namespace ExternalAdapter.Models
{
    public class InspectionSummary
    {
        public string Case { get; set; }
        public string Status { get; set; }
        public string EnityName { get; set; }
        public int SubmittedVersion { get; set; }
        public int AppliedVersion { get; set; }
        public DateTime? InspectedAt { get; set; }

        public static InspectionResult FromCases(ManagementCase[] cases)
        {
            return new InspectionResult()
            {
                Cases = cases.Select(c => new InspectionSummary
                {
                    Case = c.CaseType.ToString(),
                    Status = c.Status.ToString(),
                    EnityName = c.Name.ToString(),
                    SubmittedVersion = c.SubmitedVersion,
                    AppliedVersion = c.AppliedVersion,                    
                    InspectedAt = DateTime.UtcNow 
                }).ToList(),
                TotalCases = cases.Length
            };
        }
    }

    public class InspectionResult
    {
        public int TotalCases { get; set; }
        public List<InspectionSummary> Cases { get; set; }
    }
}

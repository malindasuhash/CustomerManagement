using Api.Services;

namespace Api.ApiModels
{
    public class ChangeSummarySubmitResult
    {
        public int total { get; set; }

        public List<ChangeSubmitResult> Changes { get; set; }
    }
}

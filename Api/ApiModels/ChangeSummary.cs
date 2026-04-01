using Api.Services;
using Microsoft.AspNetCore.Routing;

namespace Api.ApiModels
{
    public class ChangeSummary
    {
        public int total { get; set; }

        public ChangeLink[] Changes { get; set; }
    }

    public class ChangeSummarySubmitResult
    {
        public int total { get; set; }

        public List<ChangeSubmitResult> Changes { get; set; }
    }
}

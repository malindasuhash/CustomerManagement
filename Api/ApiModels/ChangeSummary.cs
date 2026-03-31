using Microsoft.AspNetCore.Routing;

namespace Api.ApiModels
{
    public class ChangeSummary
    {
        public int total { get; set; }

        public List<ChangeLink> Changes { get; set; }
    }
}

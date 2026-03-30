using StateManagment.Models;

namespace Api.ApiModels
{
    public class ChangeSummary
    {
        public int total { get; set; }

        public List<EntityBasics> Changes { get; set; }
    }
}

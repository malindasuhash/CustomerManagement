namespace Api.ApiModels
{
    public class ChangeSummary
    {
        public int total { get; set; }

        public ChangeLink[] Changes { get; set; }
    }
}

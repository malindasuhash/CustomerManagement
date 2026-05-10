namespace StateManagment.Models
{
    public class Feedback
    {
        public FeedbackType Type { get; set; }
        public Dictionary<string, string> Details { get; set; }
        public string Context { get; set; }
        public string Message { get; set; }
    }
}

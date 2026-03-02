using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public class Feedback
    {
        public FeedbackType FeedbackType { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public enum FeedbackType
    {
        None,
        Warning,
        Error
    }

    public class OrchestrationData
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public class Feedback
    {
        public FeedbackType Type { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}

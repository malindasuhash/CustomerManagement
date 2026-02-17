using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager.Models
{
    public interface IOrchestrator
    {
        Task<EvaluationResult> EvaluateAsync(MessageEnvelop messageEnvelop);
    }

    public class EvaluationResult
    {
        
    }
}

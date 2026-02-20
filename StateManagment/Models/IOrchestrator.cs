using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager.Models
{
    public interface IOrchestrator
    {
        Task<OrchestrationResult> PostApplyAsync(MessageEnvelop messageEnvelop);
        Task<OrchestrationResult> ApplyAsync(MessageEnvelop messageEnvelop);
        Task<OrchestrationResult> EvaluateAsync(MessageEnvelop messageEnvelop);
    }

    public class OrchestrationResult
    {
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager.Models
{
    public interface IOrchestrator
    {
        Task<TaskOutcome> PostApplyAsync(string entityId, EntityName entityName);
        Task<TaskOutcome> ApplyAsync(string entityId, EntityName entityName);
        Task<TaskOutcome> EvaluateAsync(string entityId, EntityName entityName);
    }

    public class OrchestrationResult
    {
        
    }
}

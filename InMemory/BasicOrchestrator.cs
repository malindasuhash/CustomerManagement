using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemory
{
    public class BasicOrchestrator : IOrchestrator
    {
        public Task<TaskOutcome> ApplyAsync(string entityId, EntityName entityName)
        {
            return Task.FromResult(TaskOutcome.OK);
        }

        public Task<TaskOutcome> EvaluateAsync(string entityId, EntityName entityName)
        {
            return Task.FromResult(TaskOutcome.OK);
        }

        public Task<TaskOutcome> PostApplyAsync(string entityId, EntityName entityName)
        {
            return Task.FromResult(TaskOutcome.OK);
        }
    }
}

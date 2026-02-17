using StateManager.Events;
using StateManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager
{
    public class StateManager
    {
        private readonly IChangeHandler changeHandler;
        private IOrchestrator orchestrator;

        public StateManager(IChangeHandler changeHandler, IOrchestrator orchestrator = null) 
        {
            this.orchestrator = orchestrator;
            this.changeHandler = changeHandler;
        }

        public Task<ChangeOutcome> ProcessChangeAsync(MessageEnvelop envelop)
        {
            // When change is created, add to repository
            if (envelop.Change == ChangeType.Create && !envelop.Submitted)
            {
                changeHandler.Draft(envelop);
                return Task.FromResult(ChangeOutcome.OK);
            }

            if (envelop.Change == ChangeType.Create && envelop.Submitted)
            {
                changeHandler.Draft(envelop);
                changeHandler.Submitted(envelop);
                orchestrator.EvaluateAsync(envelop); // Start evalutation

                return Task.FromResult(ChangeOutcome.OK);
            }

            if (envelop.Change == ChangeType.Update && !envelop.Submitted)
            {
                changeHandler.TryDraft(envelop, out ChangeOutcome outcome);
                return Task.FromResult(outcome);
            }

            if (envelop.Change == ChangeType.Update && envelop.Submitted)
            {
                if (changeHandler.TryDraft(envelop, out ChangeOutcome outcome))
                {
                    // Consider what will happen if someone is taking a copy of submitted version
                    // whilst I am trying to update it. Should I ask for a lock at this point?
                    // If cannot take the lock, should I error out?
                    changeHandler.TryLockSubmitted(envelop, out ChangeOutcome submitOutcome);
                    orchestrator.EvaluateAsync(envelop); // Start evalutation

                    return Task.FromResult(submitOutcome);
                }

                return Task.FromResult(outcome);
            }

            return Task.FromResult(ChangeOutcome.OK);
        }

        public async Task ProcessUpdateAsync(OrchestrationEnvelop orchestrationEnvelop)
        {
            throw new NotImplementedException();
        }
    }
}

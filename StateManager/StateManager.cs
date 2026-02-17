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

        public StateManager(IChangeHandler changeHandler)
        {
            this.changeHandler = changeHandler;
        }
        public Task<ProcessOutcome> ProcessChangeAsync(MessageEnvelop envelop)
        {
            // When change is created, add to repository
            if (envelop.Change == ChangeType.Create && !envelop.Submitted)
            {
                changeHandler.Draft(envelop);
                return Task.FromResult(ProcessOutcome.OK);
            }

            if (envelop.Change == ChangeType.Create && envelop.Submitted)
            {
                changeHandler.Draft(envelop);

                envelop.SubmittedVersion = envelop.DraftVersion;
                changeHandler.Submitted(envelop);

                return Task.FromResult(ProcessOutcome.OK);
            }

            if (envelop.Change == ChangeType.Update && !envelop.Submitted)
            {
                changeHandler.TryDraft(envelop, out ProcessOutcome outcome);
                return Task.FromResult(outcome);
            }

            if (envelop.Change == ChangeType.Update && envelop.Submitted)
            {
                if (changeHandler.TryDraft(envelop, out ProcessOutcome outcome))
                {
                    // Consider what will happen if someone is taking a copy of submitted version
                    // whilst I am trying to update it. Should I ask for a lock at this point?
                    // If cannot take the lock, should I error out?
                    changeHandler.TrySubmitted(envelop, out ProcessOutcome submitOutcome);
                    return Task.FromResult(submitOutcome);
                }

                return Task.FromResult(outcome);
            }

            return Task.FromResult(ProcessOutcome.OK);
        }
    }
}

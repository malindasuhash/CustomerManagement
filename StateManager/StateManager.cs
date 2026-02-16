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
        private readonly IEventManager eventManager;

        public StateManager(IEventManager eventManager)
        {
            this.eventManager = eventManager;
        }
        public void ProcessChange(MessageEnvelop envelop)
        {
            // When change is created, add to repository
            if (envelop.Change == ChangeType.Create && !envelop.Submitted) 
            { 
                eventManager.Publish(new ChangeDetected(envelop));
                return; 
            }

            if (envelop.Change == ChangeType.Create && envelop.Submitted) 
            {
                // Notifies changes to draft version
                eventManager.Publish(new ChangeDetected(envelop));

                // Notifies changes to submitted version
                envelop.SubmittedVersion = envelop.DraftVersion;
                eventManager.Publish(new ChangeSubmitted(envelop));
            }

        }
    }
}

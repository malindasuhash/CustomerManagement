using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment
{
    public class ChangeHandler : IChangeHandler
    {
        private readonly ICustomerDatabase database;
        private readonly IDistributedLock distributedLock;

        public ChangeHandler(ICustomerDatabase database, IDistributedLock distributedLock)
        {
            this.database = database;
            this.distributedLock = distributedLock;
        }

        public Task<TaskOutcome> ChangeStatusTo(string entityId, EntityName name, EntityState entityState, string[]? messages = null)
        {
            throw new NotImplementedException();
        }

        public void Draft(MessageEnvelop envelop)
        {
            database.StoreDraft(envelop.Name, envelop.Draft, envelop.EntityId); 
        }

        public Task<TaskOutcome> ReleaseEntityLock(string entityId)
        {
            var entityLock = distributedLock.Unlock(entityId);

            return entityLock;
        }

        public void Submitted(MessageEnvelop envelop)
        {
            database.StoreSubmitted(envelop.Name, envelop.Draft, envelop.EntityId);
        }

        public Task<TaskOutcome> TakeEntityLock(string entityId)
        {
            var entityLock = distributedLock.Lock(entityId);

            return entityLock;
        }

        public Task<TaskOutcome> TryDraft(MessageEnvelop envelop)
        {
            try
            {
                distributedLock.Lock($"{envelop.EntityId}_draft");
                
                var basicInfo = database.GetBasicInfo(envelop.Name, envelop.EntityId);

                // <= because if the version is same as the draft version, it means there is no new change to be made in the draft and we can avoid unnecessary update and processing.
                if (envelop.DraftVersion <= basicInfo.DraftVersion)
                {
                    return Task.FromResult(TaskOutcome.STALE_DRAFT);
                }

                database.StoreDraft(envelop.Name, envelop.Draft, envelop.EntityId);
            }
            finally {
                distributedLock.Unlock($"{envelop.EntityId}_draft");
            }
            
            return Task.FromResult(TaskOutcome.OK);
        }

        public Task<TaskOutcome> TryLockSubmitted(MessageEnvelop envelop)
        {
            throw new NotImplementedException();
        }
    }
}

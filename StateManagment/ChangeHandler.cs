using StateManagment.Models;

namespace StateManagment
{
    public class ChangeHandler : IChangeHandler
    {
        private readonly ICustomerDatabase database;
        private readonly IDistributedLock distributedLock;
        private readonly IEventPublisher eventPublisher;

        public ChangeHandler(ICustomerDatabase database, IDistributedLock distributedLock, IEventPublisher eventPublisher)
        {
            this.database = database;
            this.distributedLock = distributedLock;
            this.eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Changes the status of the specified entity in the database to the provided entity state 
        /// and publishes a state changed event.
        /// </summary>
        public Task<TaskOutcome> ChangeStatusTo(string entityId, EntityName name, EntityState entityState, string[]? messages = null)
        {
            database.UpdateData(name, entityId, entityState, messages ?? []);

            if (entityState == EntityState.SYNCHRONISED)
            {
                database.StoreApplied(name, database.GetEntityDocument(name, entityId).Submitted, entityId);
            }

            var entityDocument = database.GetEntityDocument(name, entityId);
            
            return eventPublisher.PublishStateChangedEvent(entityDocument);
        }

        /// <summary>
        /// Creates a new draft for the specified entity in the database with the provided data and
        /// increments the draft version.
        /// </summary>        
        public void Draft(MessageEnvelop envelop)
        {
            database.StoreDraft(envelop, envelop.DraftVersion + 1);
        }

        /// <summary>
        /// Releases the distributed lock held on the specified entity.
        /// </summary>
        public Task<TaskOutcome> ReleaseEntityLock(string entityId)
        {
            var entityLock = distributedLock.Unlock(entityId);

            return entityLock;
        }

        /// <summary>
        /// Stores the submitted data for the specified entity in the database along with the submitted version. 
        /// This method is called when an entity is submitted, and it updates the database with the latest submitted data 
        /// (from latest draft) and version for that entity.
        /// </summary>
        public void Submitted(MessageEnvelop envelop)
        {
            database.StoreSubmitted(envelop.Name, envelop.Draft, envelop.EntityId, envelop.UpdateUser);
        }

        /// <summary>
        /// Takes a distributed lock for the specified entity to ensure that only one process can 
        /// update or submit changes for that entity at a time.
        /// </summary>
        public Task<TaskOutcome> TakeEntityLock(string entityId)
        {
            var entityLock = distributedLock.Lock(entityId);

            return entityLock;
        }

        /// <summary>
        /// Attempts to store a draft for the specified entity, ensuring that the draft version matches the current
        /// version in the database and increments the draft version as it is being updated.
        /// </summary>      
        public Task<TaskOutcome> TryMergeDraft(MessageEnvelop envelop)
        {
            try
            {
                distributedLock.Lock($"{envelop.EntityId}_draft");

                var basicInfo = database.GetBasicInfo(envelop.Name, envelop.EntityId);

                // Draft versions must match to avoid lost updates. If the draft version in the message is different from the one in the database, it means that there has been an update since the draft was created, and we should not overwrite it.
                if (envelop.DraftVersion != basicInfo.DraftVersion)
                {
                    return Task.FromResult(TaskOutcome.VERSION_MISMATCH);
                }

                database.MergeDraft(envelop, envelop.DraftVersion + 1);
            }
            finally
            {
                distributedLock.Unlock($"{envelop.EntityId}_draft");
            }

            return Task.FromResult(TaskOutcome.OK);
        }

        /// <summary>
        /// Attempts to acquire a distributed lock for the specified entity and store its latest draft as submitted data 
        /// along with latest draft version in the database.
        /// </summary>
        public Task<TaskOutcome> TryLockSubmitted(MessageEnvelop envelop)
        {
            try
            {
                distributedLock.Lock(envelop.EntityId);

                var storedDraftEntity = database.GetEntityDocument(envelop.Name, envelop.EntityId);

                database.StoreSubmitted(storedDraftEntity.Name, storedDraftEntity.Draft, storedDraftEntity.EntityId, envelop.UpdateUser);
            }
            finally
            {
                distributedLock.Unlock(envelop.EntityId);
            }

            return Task.FromResult(TaskOutcome.OK);
        }

        public Task<TaskOutcome> Deleted(MessageEnvelop envelop)
        {
            throw new NotImplementedException();
        }
    }
}

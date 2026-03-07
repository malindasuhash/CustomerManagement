using StateManagment.Models;

namespace StateManagment
{
    public class ChangeHandler : IChangeHandler
    {
        private readonly ICustomerDatabase database;
        private readonly IDistributedLock distributedLock;
        private readonly IEventPublisher eventPublisher;
        private readonly IAuditManager auditManager;

        public ChangeHandler(ICustomerDatabase database, IDistributedLock distributedLock, IEventPublisher eventPublisher, IAuditManager auditManager)
        {
            this.database = database;
            this.distributedLock = distributedLock;
            this.eventPublisher = eventPublisher;
            this.auditManager = auditManager;
        }

        /// <summary>
        /// Changes the status of the specified entity in the database to the provided entity state 
        /// and publishes a state changed event.
        /// </summary>
        public async Task<TaskOutcome> ChangeStatusTo(string entityId, EntityName name, EntityState entityState, Feedback[]? feedbacks = null, OrchestrationData[]? orchestrationData = null)
        {
            await database.UpdateData(name, entityId, entityState, feedbacks ?? [], orchestrationData ?? []);

            if (entityState == EntityState.SYNCHRONISED)
            {
                var before = await database.GetEntityDocument(name, entityId);
                await database.StoreApplied(name, before.Submitted, entityId, before.RemoveRequested);
                var after = await database.GetEntityDocument(name, entityId);

                await auditManager.Write(AuditTarget.Applied, after, before);
            }

            var entityDocument = await database.GetEntityDocument(name, entityId);

            return await eventPublisher.DataChangedAsync(entityDocument);
        }

        /// <summary>
        /// Creates a new draft for the specified entity in the database with the provided data and
        /// increments the draft version.
        /// </summary>        
        public async Task<TaskOutcome> Draft(MessageEnvelop envelop)
        {
            await database.StoreDraft(envelop, envelop.DraftVersion + 1);

            var storedEntity = await database.GetEntityDocument(envelop.Name, envelop.EntityId);

            await auditManager.Write(AuditTarget.Draft, storedEntity);

            return await eventPublisher.DataChangedAsync(storedEntity);
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
        public async Task<TaskOutcome> Submitted(MessageEnvelop envelop)
        {
            var before = await database.GetEntityDocument(envelop.Name, envelop.EntityId);
            await database.StoreSubmitted(envelop.Name, envelop.Draft, envelop.EntityId, envelop.UpdateUser);
            var after = await database.GetEntityDocument(envelop.Name, envelop.EntityId);

            await auditManager.Write(AuditTarget.Submitted, after, before);

            return await eventPublisher.DataChangedAsync(after);
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
        /// Attempts to before a draft for the specified entity, ensuring that the draft version matches the current
        /// version in the database and increments the draft version as it is being updated.
        /// </summary>      
        public async Task<TaskOutcome> TryMergeDraft(MessageEnvelop envelop)
        {
            try
            {
                await distributedLock.Lock($"{envelop.EntityId}_draft");

                var basicInfo = await database.GetBasicInfo(envelop.Name, envelop.EntityId);

                // Draft versions must match to avoid lost updates. If the draft version in the message
                // is different from the one in the database, it means that there has been an update since the draft
                // was created, and we should not overwrite it.

                // Special case for deletes, where we'll consider consumer is intending to remove the latest draft version.
                // Therefore we'll not be checking requested version against stored version.

                var before = await database.GetEntityDocument(envelop.Name, envelop.EntityId);

                if (envelop.Change != ChangeType.Delete)
                {
                    if (envelop.DraftVersion != basicInfo.DraftVersion)
                    {
                        return TaskOutcome.VERSION_MISMATCH;
                    }
                   
                    await database.MergeDraft(envelop, envelop.DraftVersion + 1);
                }
                else
                {

                    await database.MergeDraft(envelop, basicInfo.DraftVersion + 1);
                }

                var after = await database.GetEntityDocument(envelop.Name, envelop.EntityId);

                await auditManager.Write(AuditTarget.Draft, after, before);
                return await eventPublisher.DataChangedAsync(after);
            }
            finally
            {
                await distributedLock.Unlock($"{envelop.EntityId}_draft");
            }
        }

        /// <summary>
        /// Attempts to acquire a distributed lock for the specified entity and before its latest draft as submitted data 
        /// along with latest draft version in the database.
        /// </summary>
        public async Task<TaskOutcome> TryLockSubmitted(MessageEnvelop envelop)
        {
            try
            {
                await distributedLock.Lock(envelop.EntityId);

                var before = await database.GetEntityDocument(envelop.Name, envelop.EntityId);

                // Unless there is a change, there is no need to submit
                if (before.DraftVersion == before.SubmittedVersion)
                {
                    return TaskOutcome.NO_CHANGE_TO_SUBMIT;
                }

                await database.StoreSubmitted(before.Name, before.Draft, before.EntityId, envelop.UpdateUser);
                var after = await database.GetEntityDocument(envelop.Name, envelop.EntityId);

                await auditManager.Write(AuditTarget.Submitted, after, before);
                return await eventPublisher.DataChangedAsync(after);
            }
            finally
            {
                await distributedLock.Unlock(envelop.EntityId);
            }
        }

        public async Task<TaskOutcome> TryMarkForRemoval(MessageEnvelop envelop)
        {
            try
            {
                await distributedLock.Lock(envelop.EntityId);

                var before = await database.GetEntityDocument(envelop.Name, envelop.EntityId);

                await database.MarkForRemoval(envelop.Name, envelop.EntityId);

                var after = await database.GetEntityDocument(envelop.Name, envelop.EntityId);

                await auditManager.Write(AuditTarget.Document, after, before);
                return await eventPublisher.DataChangedAsync(after);
            }
            finally
            {
                await distributedLock.Unlock(envelop.EntityId);
            }
        }
    }
}

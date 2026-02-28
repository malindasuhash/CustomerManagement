using StateManagment.Entity;
using StateManagment.Models;

namespace InMemory
{
    public class CustomerDatabase : ICustomerDatabase
    {
        private readonly EntityCollection entityCollection = new EntityCollection();

        public EntityBasics GetBasicInfo(EntityName entityName, string entityId)
        {
            var document = GetEntityDocument(entityName, entityId).Result;
            return new EntityBasics
            {
                EntityId = document.EntityId,
                Name = document.Name,
                State = document.State,
                DraftVersion = document.DraftVersion,
                SubmittedVersion = document.SubmittedVersion
            };
        }

        public Task<MessageEnvelop> GetEntityDocument(EntityName entityName, string entityId)
        {
            return entityName switch
            {
                EntityName.Contact => Task.FromResult(entityCollection.GetContact(entityId)),
                _ => throw new NotSupportedException($"Entity type {entityName} is not supported."),
            };
        }

        public void MergeDraft(MessageEnvelop envelop, int latestDraftVersion)
        {
            switch (envelop.Name)
            {
                case EntityName.Contact:
                    entityCollection.MergeContactDraft(envelop, latestDraftVersion);
                    break;
                default:
                    throw new NotSupportedException($"Entity type {envelop.Name} is not supported.");
            }
        }

        public async void StoreApplied(EntityName entityName, IEntity entity, string entityId)
        {
            switch (entityName)
            {
                case EntityName.Contact:
                    entityCollection.UpdateContactApplied(entityId, entity);
                    await Task.CompletedTask;
                    break;

                default:
                    throw new NotSupportedException($"Entity type {entityName} is not supported.");
            }
        }

        public Task<TaskOutcome> StoreDraft(MessageEnvelop messageEnvelop, int incrementalDraftVersion)
        {
            switch (messageEnvelop.Name)
            {
                case EntityName.Contact:
                    entityCollection.AddOrUpdateContact(messageEnvelop, incrementalDraftVersion);
                    break;

                default:
                    throw new NotSupportedException($"Entity type {messageEnvelop.Name} is not supported.");
            }

            return Task.FromResult(TaskOutcome.OK);
        }

        public async Task<TaskOutcome> StoreSubmitted(EntityName entityName, IEntity entity, string entityId, string updatedUser)
        {
            switch (entityName)
            {
                case EntityName.Contact:
                    entityCollection.UpdateContactSubmitted(entityId, entity, updatedUser);
                    await Task.CompletedTask;
                    break;

                default:
                    throw new NotSupportedException($"Entity type {entityName} is not supported.");
            }

            return TaskOutcome.OK;
        }

        public void UpdateData(EntityName entityName, string entityId, EntityState targetState, string[] messages)
        {
            switch (entityName)
            {
                case EntityName.Contact:
                    entityCollection.UpdateContactStateAndMessages(entityId, targetState, messages);
                    break;
                default:
                    throw new NotSupportedException($"Entity type {entityName} is not supported.");
            }
        }
    }
}

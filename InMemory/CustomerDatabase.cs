using StateManagment.Entity;
using StateManagment.Models;

namespace InMemory
{
    public class CustomerDatabase : ICustomerDatabase
    {
        private readonly EntityCollection entityCollection = new EntityCollection();

        public EntityBasics GetBasicInfo(EntityName entityName, string entityId)
        {
            var document = GetEntityDocument(entityName, entityId);
            return new EntityBasics
            {
                EntityId = document.EntityId,
                Name = document.Name,
                State = document.State,
                DraftVersion = document.DraftVersion,
                SubmittedVersion = document.SubmittedVersion
            };

            throw new NotImplementedException();
        }

        public MessageEnvelop GetEntityDocument(EntityName entityName, string entityId)
        {
            return entityName switch
            {
                EntityName.Contact => entityCollection.GetContact(entityId),
                _ => throw new NotSupportedException($"Entity type {entityName} is not supported."),
            };
        }

        public void StoreDraft(MessageEnvelop messageEnvelop, int incrementalDraftVersion)
        {
            switch (messageEnvelop.Name)
            {
                case EntityName.Contact:
                    entityCollection.AddOrUpdateContact(messageEnvelop, incrementalDraftVersion);
                    break;

                default:
                    throw new NotSupportedException($"Entity type {messageEnvelop.Name} is not supported.");
            }
        }

        public void StoreSubmitted(EntityName entityName, IEntity entity, string entityId, int latestDraftVersion)
        {
            throw new NotImplementedException();
        }

        public void UpdateData(EntityName entityName, string entityId, EntityState targetState, string[] messages)
        {
            throw new NotImplementedException();
        }
    }

    public class EntityCollection
    {
        public Dictionary<string, EntityDocument> Contacts { get; set; } = new Dictionary<string, EntityDocument>();

        public void AddOrUpdateContact(MessageEnvelop messageEnvelop, int draftVersion)
        {
            var contact = (Contact)messageEnvelop.Draft;
            if (Contacts.ContainsKey(messageEnvelop.EntityId))
            {
                Contacts[messageEnvelop.EntityId] = new EntityDocument
                {
                    EntityId = messageEnvelop.EntityId,
                    Draft = contact,
                    DraftVersion = draftVersion,
                    State = messageEnvelop.State,
                    CreatedUser = messageEnvelop.CreatedUser,
                    CreatedDate = DateTime.UtcNow,
                };
            }
        }

        public MessageEnvelop GetContact(string entityId)
        {
            if (Contacts.TryGetValue(entityId, out var document))
            {
                var message = new MessageEnvelop()
                {
                    EntityId = document.EntityId,
                    Name = EntityName.Contact,
                    Draft = document.Draft,
                    CreatedUser = document.CreatedUser,
                    CreatedDate = document.CreatedDate,
                };
                message.SetState(document.State);
            }

            throw new KeyNotFoundException($"Contact with ID {entityId} not found.");
        }
    }
}

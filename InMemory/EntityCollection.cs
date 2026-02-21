using StateManagment.Entity;
using StateManagment.Models;

namespace InMemory
{
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

        public void UpdateContactSubmitted(string entityId, IEntity entity, int latestDraftVersion, string updatedUser)
        {
            var contact = (Contact)entity;
            var contactDocument = Contacts[entityId];
            contactDocument.SubmittedVersion = latestDraftVersion;
            contactDocument.UpdatedUser = updatedUser;
            contactDocument.Submitted = new Contact()
            {
                LastName = contact.LastName,
                FirstName = contact.FirstName
            };
        }

        internal void UpdateContactApplied(string entityId, IEntity entity)
        {
            var contact = (Contact)entity;
            var contactDocument = Contacts[entityId];
            contactDocument.AppliedVersion = contactDocument.SubmittedVersion;
            contactDocument.Applied = new Contact()
            {
                LastName = contact.LastName,
                FirstName = contact.FirstName
            };
        }

        internal void UpdateContactStateAndMessages(string entityId, EntityState targetState, string[] messages)
        {
            var contactDocument = Contacts[entityId];
            contactDocument.State = targetState;
            contactDocument.Messages = [.. messages];
        }
    }
}

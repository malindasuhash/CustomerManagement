using StateManagment.Entity;
using StateManagment.Models;

namespace InMemory
{
    public class EntityCollection
    {
        public Dictionary<string, EntityDocument> Contacts { get; set; } = new Dictionary<string, EntityDocument>();

        public void AddOrUpdateContact(MessageEnvelop messageEnvelop, int draftVersion)
        {
            var entityId = Guid.NewGuid().ToString();
            messageEnvelop.EntityId = entityId;

            var contact = (Contact)messageEnvelop.Draft;
            if (!Contacts.ContainsKey(messageEnvelop.EntityId))
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
                    Change = ChangeType.Read,
                    EntityId = document.EntityId,
                    Name = EntityName.Contact,
                    Draft = document.Draft,
                    DraftVersion = document.DraftVersion,

                    UpdateUser = document.UpdatedUser,
                    UpdateTimestamp = document.UpdatedTimestamp,

                    CreatedUser = document.CreatedUser,
                    CreatedTimestamp = document.CreatedDate,
                    
                    Submitted = document.Submitted,
                    SubmittedVersion = document.SubmittedVersion,

                    Applied = document.Applied,
                    AppliedVersion = document.AppliedVersion
                };
                message.SetState(document.State);

                return message;
            }

            throw new KeyNotFoundException($"Contact with ID {entityId} not found.");
        }

        public void UpdateContactSubmitted(string entityId, IEntity entity, string updatedUser)
        {
            var contact = (Contact)entity;
            var contactDocument = Contacts[entityId];
            contactDocument.SubmittedVersion = contactDocument.DraftVersion;
            contactDocument.UpdatedUser = updatedUser;
            contactDocument.UpdatedTimestamp = DateTime.UtcNow;

            contactDocument.Submitted = new Contact()
            {
                LastName = contact.LastName,
                FirstName = contact.FirstName
            };
        }

        public void MergeContactDraft(MessageEnvelop envelop, int v)
        {
            throw new NotImplementedException();
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

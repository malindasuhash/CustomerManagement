using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StateManagment.Entity;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.EntityConfig
{
    internal static class ContactConfig
    {
        static ContactConfig()
        {
            BsonClassMap.RegisterClassMap<Contact>(cm =>
            {
                cm.AutoMap();                
            });

            BsonClassMap.RegisterClassMap<MessageEnvelop>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(a => a.EntityId);
                cm.MapMember(a => a.State).SetSerializer(new EnumSerializer<EntityState>(MongoDB.Bson.BsonType.String));
                cm.MapMember(a => a.Name).SetDefaultValue(EntityName.Contact);
                cm.MapMember(a => a.Change).SetDefaultValue(ChangeType.Read);
                cm.UnmapMember(c => c.Change);
                cm.UnmapMember(c => c.Name);
                cm.UnmapMember(c => c.IsSubmitted);
            });

            BsonClassMap.RegisterClassMap<Feedback>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(a => a.Type).SetSerializer(new EnumSerializer<FeedbackType>(MongoDB.Bson.BsonType.String));
            });

            BsonClassMap.RegisterClassMap<OrchestrationData>(cm =>
            {
                cm.AutoMap();
            });
        }

        public static async Task<DbEexecutionParams> Patch(MessageEnvelop messageEnvelop, int latestDraftVersion, IMongoDatabase db, string updatedUser = "SYSTEM")
        {
            var stored = await Get(messageEnvelop.EntityId, messageEnvelop.CustomerId, db);

            var contact = (Contact)messageEnvelop.Draft;
            var storedContact = (Contact)stored.Draft;

            if (messageEnvelop.Change == ChangeType.Update)
            {
                if (contact.FirstName != null)
                {
                    storedContact.FirstName = contact.FirstName;
                }

                if (contact.LastName != null)
                {
                    storedContact.LastName = contact.LastName;
                }
            }

            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, messageEnvelop.EntityId);
            var onUpdate = Builders<MessageEnvelop>.Update
            .Set(a => a.UpdateTimestamp, DateTime.UtcNow)
            .Set(a => a.UpdateUser, updatedUser)
            .Set(a => a.DraftVersion, latestDraftVersion)
            .Set(a => a.Draft, messageEnvelop.Change == ChangeType.Delete ? null : storedContact); // Blank out when deleting

            var contacts = db.GetCollection<MessageEnvelop>("contacts");

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onUpdate,
                Filter = filter
            };
        }

        public static async Task<DbEexecutionParams> UpdateData(string entityId, EntityState entityState, IMongoDatabase db, Feedback[] feedbacks, OrchestrationData[] orchestrationData, string updatedUser = "SYSTEM")
        {
            var contact = await GetById(entityId, db);

            // set properties
            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId);
            var onUpdate = Builders<MessageEnvelop>.Update
            .Set(a => a.State, entityState)
            .Set(b => b.Feedback, feedbacks)
            .Set(b => b.OrchestrationData, orchestrationData)
            .Set(a => a.UpdateTimestamp, DateTime.UtcNow)
            .Set(a => a.UpdateUser, updatedUser);

            var contacts = db.GetCollection<MessageEnvelop>("contacts");

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onUpdate,
                Filter = filter
            };
        }

        public static async Task<DbEexecutionParams> AddToSubmitted(IEntity entity, string entityId, string updatedUser, IMongoDatabase db)
        {
            // Read the entity document - I need the latest document here.
            var contact = await GetById(entityId, db);

            // set properties
            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId);
            var onUpdate = Builders<MessageEnvelop>.Update
            .Set(a => a.SubmittedVersion, contact.DraftVersion)
            .Set(a => a.Submitted, (Contact)contact.Draft)
            .Set(a => a.UpdateTimestamp, DateTime.UtcNow)
            .Set(a => a.UpdateUser, updatedUser);

            var contacts = db.GetCollection<MessageEnvelop>("contacts");

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onUpdate,
                Filter = filter
            };
        }

        public static async Task<DbEexecutionParams> AddToApplied(string entityId, IEntity entity, bool confirmRemoval, IMongoDatabase db, string updatedUser = "SYSTEM")
        {
            var contact = await GetById(entityId, db);

            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId);

            var onUpdate = Builders<MessageEnvelop>.Update;
            var definition = onUpdate.Set(a => a.AppliedVersion, contact.SubmittedVersion)
            .Set(a => a.Applied, (Contact)entity)
            .Set(a => a.UpdateTimestamp, DateTime.UtcNow)
            .Set(a => a.UpdateUser, updatedUser)
            .Set(a => a.Removed, confirmRemoval);

            var contacts = db.GetCollection<MessageEnvelop>("contacts");

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = definition,
                Filter = filter
            };
        }

        public static Task<DbEexecutionParams> AddToDraft(MessageEnvelop messageEnvelop, int incrementalDraftVersion, IMongoDatabase db)
        {
            messageEnvelop.EntityId = Guid.NewGuid().ToString();
            messageEnvelop.DraftVersion = incrementalDraftVersion;

            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, messageEnvelop.EntityId);
            var onInsert = Builders<MessageEnvelop>.Update
            .Set(a => a.DraftVersion, messageEnvelop.DraftVersion)
            .Set(a => a.SubmittedVersion, messageEnvelop.SubmittedVersion)
            .Set(a => a.AppliedVersion, messageEnvelop.AppliedVersion)
            .Set(a => a.Draft, (Contact)messageEnvelop.Draft)
            .Set(a => a.State, messageEnvelop.State)
            .Set(a => a.CustomerId, messageEnvelop.CustomerId)
            .Set(a => a.RemoveRequested, false)
            .Set(a => a.Removed, false)
            .SetOnInsert(a => a.CreatedTimestamp, DateTime.UtcNow)
            .SetOnInsert(a => a.CreatedUser, messageEnvelop.CreatedUser)
            .SetOnInsert(a => a.EntityId, messageEnvelop.EntityId);

            var contacts = db.GetCollection<MessageEnvelop>("contacts");

            return Task.FromResult(new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onInsert,
                Filter = filter
            });
        }

        public static Task<DbEexecutionParams> SetMarkForRemoval(string entityId, IMongoDatabase db)
        {
            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId);
            var onInsert = Builders<MessageEnvelop>.Update
            .Set(a => a.RemoveRequested, true);

            var contacts = db.GetCollection<MessageEnvelop>("contacts");

            return Task.FromResult(new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onInsert,
                Filter = filter
            });
        }

        public static Task<MessageEnvelop> Get(string entityId, string customerId, IMongoDatabase db)
        {
            var filter = Builders<MessageEnvelop>.Filter.And(
                Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId),
                Builders<MessageEnvelop>.Filter.Eq(o => o.CustomerId, customerId));
                

            var contacts = db.GetCollection<MessageEnvelop>("contacts");

            return contacts.Find(filter).FirstOrDefaultAsync();
        }

        public static Task<MessageEnvelop> GetById(string entityId, IMongoDatabase db)
        {
            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId);

            var contacts = db.GetCollection<MessageEnvelop>("contacts");

            return contacts.Find(filter).FirstOrDefaultAsync();
        }

        public static Task<EntityBasics> GetEntityBasics(string entityId, IMongoDatabase db)
        {
            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId);

            var contacts = db.GetCollection<MessageEnvelop>("contacts");

            return contacts.Find(filter)
                .Project(p => new EntityBasics
                {
                    DraftVersion = p.DraftVersion,
                    EntityId = entityId,
                    Name = EntityName.Contact,
                    State = p.State,
                    SubmittedVersion = p.SubmittedVersion,
                }).FirstOrDefaultAsync();
        }

        public static void CreateDataChangedEvent(MessageEnvelop messageEnvelop)
        {

        }
    }
}

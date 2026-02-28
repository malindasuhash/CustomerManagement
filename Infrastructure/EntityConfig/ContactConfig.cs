using MongoDB.Bson.Serialization;
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
            BsonClassMap.RegisterClassMap<MessageEnvelop>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.Change);
                cm.UnmapMember(c => c.Name);
                cm.UnmapMember(c => c.IsSubmitted);
            });
        }

        public static async Task<DbEexecutionParams> Add(MessageEnvelop messageEnvelop, int incrementalDraftVersion, IMongoDatabase db)
        {
            messageEnvelop.EntityId = Guid.NewGuid().ToString();
            messageEnvelop.DraftVersion = incrementalDraftVersion;

            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, messageEnvelop.EntityId);
            var onInsert = Builders<MessageEnvelop>.Update
            .Set(a => a.DraftVersion, messageEnvelop.DraftVersion)
            .Set(a => a.SubmittedVersion, messageEnvelop.SubmittedVersion)
            .Set(a => a.AppliedVersion, messageEnvelop.AppliedVersion)
            .Set(a => a.Draft, (Contact)messageEnvelop.Draft)
            .SetOnInsert(a => a.CreatedTimestamp, DateTime.UtcNow)
            .SetOnInsert(a => a.CreatedUser, messageEnvelop.CreatedUser)
            .SetOnInsert(a => a.EntityId, messageEnvelop.EntityId);

            var contacts = db.GetCollection<MessageEnvelop>("contacts");

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onInsert,
                Filter = filter
            };
        }
    }
}

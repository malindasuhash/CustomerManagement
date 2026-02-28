using Infrastructure.EntityConfig;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StateManagment.Entity;
using StateManagment.Models;

namespace Infrastructure
{
    public class MongoCustomerDatabase : ICustomerDatabase
    {
        static MongoCustomerDatabase()
        {
            var objectSerializer = new ObjectSerializer(ObjectSerializer.AllAllowedTypes);
            BsonSerializer.RegisterSerializer(objectSerializer);
        }

        public EntityBasics GetBasicInfo(EntityName entityName, string entityId)
        {
            throw new NotImplementedException();
        }

        public MessageEnvelop GetEntityDocument(EntityName entityName, string entityId)
        {
            throw new NotImplementedException();
        }

        public void MergeDraft(MessageEnvelop envelop, int latestDraftVersion)
        {
            throw new NotImplementedException();
        }

        public void StoreApplied(EntityName entityName, IEntity entity, string entityId)
        {
            throw new NotImplementedException();
        }

        public async Task<TaskOutcome> StoreDraft(MessageEnvelop messageEnvelop, int incrementalDraftVersion)
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoConnectionString"));
            var db = client.GetDatabase("customers");

            var result = ContactConfig.Add(messageEnvelop, incrementalDraftVersion, db).Result;

            await result.Collection.UpdateOneAsync(result.Filter, result.Definition, new UpdateOptions { IsUpsert = true });

            return TaskOutcome.OK;
        }

        public void StoreSubmitted(EntityName entityName, IEntity entity, string entityId, string updatedUser)
        {
            throw new NotImplementedException();
        }

        public void UpdateData(EntityName entityName, string entityId, EntityState targetState, string[] messages)
        {
            throw new NotImplementedException();
        }
    }
}

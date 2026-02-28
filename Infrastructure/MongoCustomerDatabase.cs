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
        private readonly IMongoDatabase database;

        public MongoCustomerDatabase()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoConnectionString"));
            database = client.GetDatabase("customers");
        }

        static MongoCustomerDatabase()
        {
            var objectSerializer = new ObjectSerializer(ObjectSerializer.AllAllowedTypes);
            BsonSerializer.RegisterSerializer(objectSerializer);
        }

        public EntityBasics GetBasicInfo(EntityName entityName, string entityId)
        {
            throw new NotImplementedException();
        }

        public async Task<MessageEnvelop> GetEntityDocument(EntityName entityName, string entityId)
        {
            switch (entityName)
            {
                case EntityName.Contact:
                    var contact = await ContactConfig.Get(entityId, database);
                    contact.Name = entityName;
                    contact.Change = ChangeType.Read;
                    return contact;
            }

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
            DbEexecutionParams dbEexecution;

            switch (messageEnvelop.Name)
            {
                case EntityName.Contact:
                    dbEexecution = await ContactConfig.Add(messageEnvelop, incrementalDraftVersion, database);
                    break;

                default:
                    throw new NotImplementedException();
            }

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition, new UpdateOptions { IsUpsert = true });

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

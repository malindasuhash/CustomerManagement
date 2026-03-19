using Infrastructure.EntityConfig;
using MongoDB.Bson.Serialization;
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

        public async Task<EntityBasics> GetBasicInfo<T>(LookupPredicate predicate) where T : IEntity
        {
            var storedBasics = await DatabaseCollectionConfig.GetEntityBasics<T>(predicate.EntityId, predicate.CustomerId, predicate.LegalEntityId, database);
            return storedBasics;
        }

        public async Task<TaskOutcome> MergeDraft<T>(MessageEnvelop envelop, int latestDraftVersion) where T : IEntity
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.Patch<T>(envelop, latestDraftVersion, database);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> StoreApplied<T>(IEntity entity, string entityId, string customerId, bool confirmRemoval) where T : IEntity
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.AddToApplied<T>(entityId, customerId, entity, confirmRemoval, database);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> StoreDraft<T>(MessageEnvelop messageEnvelop, int incrementalDraftVersion) where T : IEntity
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.AddToDraft<T>(messageEnvelop, incrementalDraftVersion, database);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition, new UpdateOptions { IsUpsert = true });

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> StoreSubmitted<T>(LookupPredicate predicate, string updatedUser) where T : IEntity
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.AddToSubmitted<T>(predicate.EntityId, predicate.CustomerId, predicate.LegalEntityId, updatedUser, database);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> UpdateData<T>(string entityId, string customerId, EntityState targetState, Feedback[] feedbacks, OrchestrationData[]? orchestrationData = null, string? legalEntityId = null) where T : IEntity
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.UpdateData<T>(entityId, customerId, targetState, database, feedbacks, orchestrationData);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> MarkForRemoval<T>(string entityId) where T : IEntity
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.SetMarkForRemoval<T>(entityId, database);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<MessageEnvelop> GetEntity<T>(LookupPredicate lookupPredicate) where T : IEntity
        {
            var storedEntity = await DatabaseCollectionConfig.GetById2<T>(lookupPredicate.EntityId, lookupPredicate.CustomerId, database, lookupPredicate.LegalEntityId);
            storedEntity.Name = EntityCollectionConfig.Config<T>().Name;
            storedEntity.Change = ChangeType.Read;
         
            return storedEntity;
        }
    }
}

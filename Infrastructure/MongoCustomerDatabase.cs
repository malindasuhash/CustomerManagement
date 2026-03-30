using Infrastructure.EntityConfig;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StateManagment.Entity;
using StateManagment.Models;
using System;
using System.IO.MemoryMappedFiles;

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
            DatabaseCollectionConfig.Run();
        }

        public async Task<EntityBasics> GetBasicInfo<T>(LookupPredicate predicate) where T : IEntity
        {
            var storedBasics = await DatabaseCollectionConfig.GetEntityBasics<T>(predicate, database);
            return storedBasics;
        }

        public async Task<TaskOutcome> MergeDraft<T>(MessageEnvelop envelop, int latestDraftVersion) where T : IEntity
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.Patch<T>(envelop, latestDraftVersion, database);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> StoreApplied<T>(LookupPredicate predicate, bool confirmRemoval) where T : IEntity
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.AddToApplied<T>(predicate, confirmRemoval, database);

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

            dbEexecution = await DatabaseCollectionConfig.AddToSubmitted<T>(predicate, updatedUser, database);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> UpdateData<T>(LookupPredicate predicate, EntityState targetState, Feedback[] feedbacks, OrchestrationData[]? orchestrationData = null) where T : IEntity
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.UpdateData<T>(predicate, targetState, database, feedbacks, orchestrationData);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> MarkForRemoval<T>(LookupPredicate predicate) where T : IEntity
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.SetMarkForRemoval<T>(predicate, database);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<MessageEnvelop> FindEntity<T>(LookupPredicate lookupPredicate) where T : IEntity
        {
            var storedEntity = await DatabaseCollectionConfig.FindBy<T>(lookupPredicate, database);

            if (storedEntity == null)
            {
                // Entity not found
                return MessageEnvelop.NONE;
            }

            storedEntity.Name = EntityCollectionConfig.Config<T>().Name;
            storedEntity.Change = ChangeType.Read;

            return storedEntity;
        }

        public async Task<List<EntityBasics>> GetPendingChanges(string customerId, string? legalEntityId = null)
        {
            var pendingChanges = new List<EntityBasics>();

            var collections = new[]
            {
                    EntityCollectionConfig.Config<Contact>(),
                    EntityCollectionConfig.Config<LegalEntity>(),
                    EntityCollectionConfig.Config<BillingGroup>(),
                    EntityCollectionConfig.Config<BankAccount>(),
                    EntityCollectionConfig.Config<ProductAgreement>(),
                    EntityCollectionConfig.Config<TradingLocation>()
                };

            var filter = Builders<MessageEnvelop>.Filter;

            foreach (var entityMap in collections)
            {
                var collection = database.GetCollection<MessageEnvelop>(entityMap.Collection);

                var filterDef = filter.And(
                    filter.Eq(o => o.CustomerId, customerId),
                    filter.Where(o => o.DraftVersion != o.SubmittedVersion)
                );

                if (legalEntityId != null)
                {
                    filterDef = filter.And(filterDef, filter.Eq("Draft.LegalEntityId", legalEntityId));
                }

                var results = await collection
                    .Find(filterDef)
                    .ToListAsync();

                pendingChanges.AddRange(results.Select(p => new EntityBasics
                {
                    EntityId = p.EntityId,
                    Name = entityMap.Name,
                    DraftVersion = p.DraftVersion,
                    SubmittedVersion = p.SubmittedVersion,
                    State = p.State
                }));
            }

            return pendingChanges;
        }
    }
}

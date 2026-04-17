using Infrastructure.EntityConfig;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StateManagment.Entity;
using StateManagment.Models;

namespace Infrastructure
{
    public class MongoCustomerDatabase : ICustomerDatabase, IDistributedLock
    {
        private readonly IMongoDatabase database;
        private readonly IMongoCollection<BsonDocument> configCollection;

        public MongoCustomerDatabase()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoConnectionString"));
            database = client.GetDatabase("customers");
            configCollection = database.GetCollection<BsonDocument>("lock-config");
            EnsureTtlIndex();
        }

        static MongoCustomerDatabase()
        {
            var objectSerializer = new ObjectSerializer(ObjectSerializer.AllAllowedTypes);
            BsonSerializer.RegisterSerializer(objectSerializer);
            DatabaseCollectionConfig.Run();
        }

        private void EnsureTtlIndex()
        {
            var indexKeys = Builders<BsonDocument>.IndexKeys.Ascending("ExpiresAt");
            var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
            configCollection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(indexKeys, indexOptions));
        }

        public async Task<TaskOutcome> TakeLock(string lockOnKey, int timeInSeconds = 10)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", lockOnKey);
            var expiresAt = DateTime.UtcNow.AddSeconds(timeInSeconds);

            var update = Builders<BsonDocument>.Update
                .SetOnInsert("_id", lockOnKey)
                .SetOnInsert("ExpiresAt", expiresAt);

            try
            {
                await configCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
                return TaskOutcome.OK;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // Attempt to remove the lock if it has already expired
                var expiredFilter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("_id", lockOnKey),
                    Builders<BsonDocument>.Filter.Lt("ExpiresAt", DateTime.UtcNow)
                );

                var deleteResult = await configCollection.DeleteOneAsync(expiredFilter);

                if (deleteResult.DeletedCount > 0)
                {
                    // Expired lock was removed, try to acquire a new one
                    try
                    {
                        await configCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
                        return TaskOutcome.OK;
                    }
                    catch (MongoWriteException retryEx) when (retryEx.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    {
                        // Another process acquired the lock between delete and insert
                        return TaskOutcome.LOCK_UNAVAILABLE;
                    }
                }

                return TaskOutcome.LOCK_UNAVAILABLE;
            }
        }

        public async Task<TaskOutcome> ReleaseLock(string lockOnKey)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", lockOnKey);
            await configCollection.DeleteOneAsync(filter);
            return TaskOutcome.OK;
        }

        async Task<TaskOutcome> IDistributedLock.Lock(string key)
        {
            return await TakeLock(key);
        }

        async Task<TaskOutcome> IDistributedLock.Unlock(string key)
        {
            return await ReleaseLock(key);
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

        // Added method: GetLegalEntitiesBy
        public async Task<List<MessageEnvelop>> GetLegalEntitiesBy(string customerId, string contactId)
        {
            var entityMap = EntityCollectionConfig.Config<LegalEntity>();
            var collection = database.GetCollection<BsonDocument>(entityMap.Collection);

            // Build element match for business contacts in Draft / Submitted / Applied
            var contactMatch = Builders<BsonDocument>.Filter.Eq("ContactId", contactId);
            var submittedMatch = Builders<BsonDocument>.Filter.ElemMatch("Submitted.BusinessContacts", contactMatch);

            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("CustomerId", customerId),
                Builders<BsonDocument>.Filter.And(submittedMatch)
            );

            var docs = await collection.Find(filter).ToListAsync();

            var results = docs.Select(d =>
            {
                var env = BsonSerializer.Deserialize<MessageEnvelop>(d);
                env.Name = entityMap.Name;
                env.Change = ChangeType.Read;
                return env;
            }).ToList();

            return results;
        }
    }
}

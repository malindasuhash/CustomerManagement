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

        public async Task<EntityBasics> GetBasicInfo(EntityName entityName, string entityId)
        {
            var storedBasics = await DatabaseCollectionConfig.GetEntityBasics(entityId, entityName, database);
            return storedBasics;
        }

        public async Task<MessageEnvelop> GetEntityDocument(EntityName entityName, string entityId, string? customerId = null)
        {
            var storedEntity = await DatabaseCollectionConfig.GetById(entityId, entityName, database);
            storedEntity.Name = entityName;
            storedEntity.Change = ChangeType.Read;
            return storedEntity;
        }

        public async Task<TaskOutcome> MergeDraft(MessageEnvelop envelop, int latestDraftVersion)
        {
            DbEexecutionParams dbEexecution;

            switch (envelop.Name)
            {
                case EntityName.Contact:
                    dbEexecution = await DatabaseCollectionConfig.Patch<Contact>(envelop, latestDraftVersion, database);
                    break;

                case EntityName.LegalEntity:
                    dbEexecution = await DatabaseCollectionConfig.Patch<LegalEntity>(envelop, latestDraftVersion, database);
                    break;

                case EntityName.BillingGroup:
                    dbEexecution = await DatabaseCollectionConfig.Patch<BillingGroup>(envelop, latestDraftVersion, database);
                    break;

                case EntityName.BankAccount:
                    dbEexecution = await DatabaseCollectionConfig.Patch<BankAccount>(envelop, latestDraftVersion, database);
                    break;

                case EntityName.ProductAgreement:
                    dbEexecution = await DatabaseCollectionConfig.Patch<ProductAgreement>(envelop, latestDraftVersion, database);
                    break;

                default:
                    throw new NotImplementedException();
            }

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> StoreApplied(EntityName entityName, IEntity entity, string entityId, bool confirmRemoval)
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.AddToApplied(entityId, entity, confirmRemoval, database, entityName);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> StoreDraft(MessageEnvelop messageEnvelop, int incrementalDraftVersion)
        {
            DbEexecutionParams dbEexecution;

            switch (messageEnvelop.Name)
            {
                case EntityName.Contact:
                    dbEexecution = await DatabaseCollectionConfig.AddToDraft<Contact>(messageEnvelop, incrementalDraftVersion, database);
                    break;

                case EntityName.LegalEntity:
                    dbEexecution = await DatabaseCollectionConfig.AddToDraft<LegalEntity>(messageEnvelop, incrementalDraftVersion, database);
                    break;

                case EntityName.BillingGroup:
                    dbEexecution = await DatabaseCollectionConfig.AddToDraft<BillingGroup>(messageEnvelop, incrementalDraftVersion, database);
                    break;

                case EntityName.BankAccount:
                    dbEexecution = await DatabaseCollectionConfig.AddToDraft<BankAccount>(messageEnvelop, incrementalDraftVersion, database);
                    break;

                case EntityName.ProductAgreement:
                    dbEexecution = await DatabaseCollectionConfig.AddToDraft<ProductAgreement>(messageEnvelop, incrementalDraftVersion, database);
                    break;

                default:
                    throw new NotImplementedException();
            }

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition, new UpdateOptions { IsUpsert = true });

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> StoreSubmitted(EntityName entityName, IEntity entity, string entityId, string updatedUser)
        {
            DbEexecutionParams dbEexecution;

            switch (entityName)
            {
                case EntityName.Contact:
                    dbEexecution = await DatabaseCollectionConfig.AddToSubmitted<Contact>(entity, entityId, updatedUser, entityName, database);
                    break;

                case EntityName.LegalEntity:
                    dbEexecution = await DatabaseCollectionConfig.AddToSubmitted<LegalEntity>(entity, entityId, updatedUser, entityName, database);
                    break;

                case EntityName.BillingGroup:
                    dbEexecution = await DatabaseCollectionConfig.AddToSubmitted<BillingGroup>(entity, entityId, updatedUser, entityName, database);
                    break;

                case EntityName.BankAccount:
                    dbEexecution = await DatabaseCollectionConfig.AddToSubmitted<BankAccount>(entity, entityId, updatedUser, entityName, database);
                    break;

                case EntityName.ProductAgreement:
                    dbEexecution = await DatabaseCollectionConfig.AddToSubmitted<ProductAgreement>(entity, entityId, updatedUser, entityName, database);
                    break;

                default:
                    throw new NotImplementedException();
            }

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> UpdateData(EntityName entityName, string entityId, EntityState targetState, Feedback[] feedbacks, OrchestrationData[]? orchestrationData = null)
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.UpdateData(entityName, entityId, targetState, database, feedbacks, orchestrationData);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> MarkForRemoval(EntityName name, string entityId)
        {
            DbEexecutionParams dbEexecution;

            dbEexecution = await DatabaseCollectionConfig.SetMarkForRemoval(entityId, name, database);

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<MessageEnvelop> GetEntity<T>(string entityId, string? customerId = null) where T : IEntity
        {
            var storedEntity = await DatabaseCollectionConfig.GetById2<T>(entityId, database);
            storedEntity.Name = EntityCollectionConfig.Config<T>().Name;
            storedEntity.Change = ChangeType.Read;
            return storedEntity;
            throw new NotImplementedException();
        }
    }
}

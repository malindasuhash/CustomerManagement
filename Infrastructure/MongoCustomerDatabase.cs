using Infrastructure.EntityConfig;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SharpCompress.Common;
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
            switch (entityName)
            {
                case EntityName.Contact:
                    var contact = await ContactConfig.GetEntityBasics(entityId, database);
                    return contact;
            }

            throw new NotImplementedException();
        }

        public async Task<MessageEnvelop> GetEntityDocument(EntityName entityName, string entityId, string? customerId = null)
        {
            switch (entityName)
            {
                case EntityName.Contact:
                    var contact = await ContactConfig.GetById(entityId, "contacts", database);
                    contact.Name = entityName;
                    contact.Change = ChangeType.Read;
                    return contact;

                case EntityName.LegalEntity:
                    var legalEntity = await ContactConfig.GetById(entityId, "legal-entities", database);
                    legalEntity.Name = entityName;
                    legalEntity.Change = ChangeType.Read;
                    return legalEntity;
            }

            throw new NotImplementedException();
        }

        public async Task<TaskOutcome> MergeDraft(MessageEnvelop envelop, int latestDraftVersion)
        {
            DbEexecutionParams dbEexecution;

            switch (envelop.Name)
            {
                case EntityName.Contact:
                    dbEexecution = await ContactConfig.Patch(envelop, latestDraftVersion, database);
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

            switch (entityName)
            {
                case EntityName.Contact:
                    dbEexecution = await ContactConfig.AddToApplied(entityId, entity, confirmRemoval, database);
                    break;

                default:
                    throw new NotImplementedException();
            }

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> StoreDraft(MessageEnvelop messageEnvelop, int incrementalDraftVersion)
        {
            DbEexecutionParams dbEexecution;

            switch (messageEnvelop.Name)
            {
                case EntityName.Contact:
                    dbEexecution = await ContactConfig.AddToDraft<Contact>(messageEnvelop, incrementalDraftVersion, "contacts", database);
                    break;

                case EntityName.LegalEntity:
                    dbEexecution = await ContactConfig.AddToDraft<LegalEntity>(messageEnvelop, incrementalDraftVersion, "legal-entities", database);
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
                    dbEexecution = await ContactConfig.AddToSubmitted(entity, entityId, updatedUser, database);
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

            switch (entityName)
            {
                case EntityName.Contact:
                    dbEexecution = await ContactConfig.UpdateData(entityId, targetState, database, feedbacks, orchestrationData);
                    break;

                default:
                    throw new NotImplementedException();
            }

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }

        public async Task<TaskOutcome> MarkForRemoval(EntityName name, string entityId)
        {
            DbEexecutionParams dbEexecution;

            switch (name)
            {
                case EntityName.Contact:
                    dbEexecution = await ContactConfig.SetMarkForRemoval(entityId, database);
                    break;

                default:
                    throw new NotImplementedException();
            }

            await dbEexecution.Collection.UpdateOneAsync(dbEexecution.Filter, dbEexecution.Definition);

            return TaskOutcome.OK;
        }
    }
}

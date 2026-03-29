using ExternalAdapter.Interfaces;
using ExternalAdapter.Services.AmendContact;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Infrastructure
{
    public class AdapterDatabase : IAdapterDatabase
    {
        private const string AdapterExternalDatabase = "adapter-external";
        private const string CollectionName = "management-case";

        private readonly IMongoCollection<ManagementCase> _collection;

        static AdapterDatabase()
        {
            BsonClassMap.RegisterClassMap<Contact>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<ManagementCase>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
                cm.MapMember(c => c.CaseType).SetSerializer(new EnumSerializer<CaseType>(BsonType.String));
                cm.MapMember(c => c.Status).SetSerializer(new EnumSerializer<CaseStatus>(BsonType.String));
                cm.MapMember(c => c.Name).SetSerializer(new EnumSerializer<EntityName>(BsonType.String));
                cm.MapMember(c => c.EntitiesToReevaluate)
  .SetSerializer(new EnumerableInterfaceImplementerSerializer<List<EntityName>>(new EnumSerializer<EntityName>(BsonType.String)));
            });
        }

        public AdapterDatabase()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoConnectionString"));
            _collection = client.GetDatabase(AdapterExternalDatabase).GetCollection<ManagementCase>(CollectionName);
        }

        public void RegisterChanges(List<ManagementCase> managementCases)
        {
            foreach (var managementCase in managementCases)
            {
                var filter = Builders<ManagementCase>.Filter.And(
                    Builders<ManagementCase>.Filter.Eq(c => c.Checksum, managementCase.Checksum),
                    Builders<ManagementCase>.Filter.Eq(c => c.CaseType, managementCase.CaseType)
                );

                var existingCase = _collection.Find(filter).FirstOrDefault();

                if (existingCase != null)
                    continue;

                managementCase.Id = Guid.NewGuid().ToString();
                managementCase.CreateDateTime = DateTime.UtcNow;
                managementCase.LastUpdated = DateTime.UtcNow;

                _collection.InsertOne(managementCase);
            }
        }

        public List<ManagementCase> FindCases(string checksum)
        {
            var filter = Builders<ManagementCase>.Filter.And(
                Builders<ManagementCase>.Filter.Eq(c => c.Checksum, checksum),
                Builders<ManagementCase>.Filter.In(c => c.Status, new[]
                {
                        CaseStatus.Open,
                        CaseStatus.Candidate,
                        CaseStatus.Pending
                })
            );

            return _collection.Find(filter).ToList();
        }

        public List<ManagementCase> FindCasesBy(string customerId, string legalEntityId, string entityId)
        {
            var filter = Builders<ManagementCase>.Filter.And(
                Builders<ManagementCase>.Filter.Eq("Identifiers.CustomerId", customerId),
                Builders<ManagementCase>.Filter.Eq("Identifiers.LegalEntityId", legalEntityId),
                Builders<ManagementCase>.Filter.Eq("Identifiers.EntityId", entityId),
                Builders<ManagementCase>.Filter.In(c => c.Status, new[]
                {
                        CaseStatus.Open,
                        CaseStatus.Candidate,
                        CaseStatus.Pending
                })
            );

            return _collection.Find(filter).ToList();
        }
    }
}
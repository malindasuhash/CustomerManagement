using MongoDB.Driver;
using StateManagment.Models;

namespace Infrastructure.Repositories
{
    public class CustomerProfileRepository : ICustomerProfileRepository
    {
        private readonly IMongoDatabase database;
        private readonly EntityCollectionConfig.EntityMap map;

        public CustomerProfileRepository()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable(Statics.MongoConnectionStringEnv));
            database = client.GetDatabase(Statics.DatabaseName);
            map = EntityCollectionConfig.Config<CustomerProfile>();

            // Ensure unique index on CustomerId at repository construction time (one-time operation)
            try
            {
                var customerProfiles = database.GetCollection<CustomerProfile>(map.Collection);
                var indexKeys = Builders<CustomerProfile>.IndexKeys.Ascending(c => c.CustomerId);
                var indexModel = new CreateIndexModel<CustomerProfile>(indexKeys, new CreateIndexOptions { Unique = true });
                // CreateOne is synchronous and acceptable at startup; ignore errors to avoid blocking startup on index issues
                customerProfiles.Indexes.CreateOne(indexModel);
            }
            catch
            {
                // ignore index creation failures
            }
        }

        public async Task<TaskOutcome> Create(CustomerProfile customerProfile)
        {
            var customerProfiles = database.GetCollection<CustomerProfile>(map.Collection);

            // Index creation moved to constructor; proceed with insertion logic
            if (customerProfile == null || string.IsNullOrWhiteSpace(customerProfile.CustomerId))
            {
                return TaskOutcome.FAILED;
            }

            // Check for existing CustomerId to avoid duplicate key error
            var filter = Builders<CustomerProfile>.Filter.Eq(c => c.CustomerId, customerProfile.CustomerId);
            var existing = await customerProfiles.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
            if (existing != null)
            {
                return TaskOutcome.FAILED;
            }

            // Ensure internal Id is set
            if (string.IsNullOrWhiteSpace(customerProfile.Id))
            {
                customerProfile.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            }

            try
            {
                await customerProfiles.InsertOneAsync(customerProfile).ConfigureAwait(false);
                return TaskOutcome.OK;
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return TaskOutcome.FAILED;
            }
            catch
            {
                return TaskOutcome.FAILED;
            }
        }

        public async Task<TaskOutcome> Delete(string customerId)
        {
            var customerProfiles = database.GetCollection<CustomerProfile>(map.Collection);

            var filter = Builders<CustomerProfile>.Filter.Eq(c => c.CustomerId, customerId);
            var result = await customerProfiles.DeleteOneAsync(filter).ConfigureAwait(false);

            if (result.DeletedCount == 0)
            {
                return TaskOutcome.NOT_FOUND;
            }

            return TaskOutcome.OK;
        }

        public async Task<CustomerProfile?> Read(string customerId)
        {
            var customerProfiles = database.GetCollection<CustomerProfile>(map.Collection);

            var filter = Builders<CustomerProfile>.Filter.Eq(c => c.CustomerId, customerId);
            var result = await customerProfiles.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);

            return result;
        }

        public async Task<TaskOutcome> Update(CustomerProfile customerProfile)
        {
            var customerProfiles = database.GetCollection<CustomerProfile>(map.Collection);

            // Find existing to preserve internal Id if not supplied
            var filter = Builders<CustomerProfile>.Filter.Eq(c => c.CustomerId, customerProfile.CustomerId);
            var existing = await customerProfiles.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
            if (existing == null)
            {
                return TaskOutcome.NOT_FOUND;
            }

            // Preserve internal Id
            customerProfile.Id = existing.Id;

            if (customerProfile.Name != null)
            {
                existing.Name = customerProfile.Name;
            }

            if (customerProfile.SystemData != null)
            {
                existing.SystemData = customerProfile.SystemData;
            }

            if (customerProfile.MetaData != null)
            {
                existing.MetaData = customerProfile.MetaData;
            }

            if (customerProfile.Labels != null)
            {
                existing.Labels = customerProfile.Labels;
            }

            existing.UpdateTimestamp = customerProfile.UpdateTimestamp;
            existing.UpdateUser = customerProfile.UpdateUser;

            var replaceResult = await customerProfiles.ReplaceOneAsync(filter, existing).ConfigureAwait(false);
            if (replaceResult.IsAcknowledged && replaceResult.ModifiedCount >= 0)
            {
                return TaskOutcome.OK;
            }

            return TaskOutcome.FAILED;
        }
    }
}

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
        }

        public async Task<TaskOutcome> Create(CustomerProfile customerProfile)
        {
            var customerProfiles = database.GetCollection<CustomerProfile>(map.Collection);

            // Ensure unique index on CustomerId
            var indexKeys = Builders<CustomerProfile>.IndexKeys.Ascending(c => c.CustomerId);
            var indexModel = new CreateIndexModel<CustomerProfile>(indexKeys, new CreateIndexOptions { Unique = true });
            customerProfiles.Indexes.CreateOne(indexModel);

            try
            {
                // Insert; if duplicate CustomerId exists, driver will throw
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
            var customerProfiles = database.GetCollection<CustomerProfile>("CustomerProfiles");

            var filter = Builders<CustomerProfile>.Filter.Eq(c => c.CustomerId, customerId);
            var result = await customerProfiles.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);

            return result;
        }

        public async Task<TaskOutcome> Update(CustomerProfile customerProfile)
        {
            var customerProfiles = database.GetCollection<CustomerProfile>("CustomerProfiles");

            // Find existing to preserve internal Id if not supplied
            var filter = Builders<CustomerProfile>.Filter.Eq(c => c.CustomerId, customerProfile.CustomerId);
            var existing = await customerProfiles.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
            if (existing == null)
            {
                return TaskOutcome.NOT_FOUND;
            }

            // Preserve internal Id
            customerProfile.Id = existing.Id;

            var replaceResult = await customerProfiles.ReplaceOneAsync(filter, customerProfile).ConfigureAwait(false);
            if (replaceResult.IsAcknowledged && replaceResult.ModifiedCount >= 0)
            {
                return TaskOutcome.OK;
            }

            return TaskOutcome.FAILED;
        }
    }
}

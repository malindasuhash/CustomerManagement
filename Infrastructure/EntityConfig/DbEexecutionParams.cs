using MongoDB.Driver;
using StateManagment.Models;

namespace Infrastructure.EntityConfig
{
    internal class DbEexecutionParams
    {
        public IMongoCollection<MessageEnvelop> Collection { get; set; }

        public UpdateDefinition<MessageEnvelop> Definition { get; set; }

        public FilterDefinition<MessageEnvelop> Filter { get; set; }
    }
}

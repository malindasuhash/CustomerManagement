using MongoDB.Bson.IO;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Models
{
    public class ChangeModel<T> where T : IEntity
    {
        public string Origin { get; set; }
        public string CorellationId { get; set; } = Guid.NewGuid().ToString();
        public string EntityId { get; set; }
        public string CustomerId { get; set; }
        public string LegalEntityId { get; set; }
        public T Submitted { get; set; }
        public T Applied { get; set; }
        public int SubmittedVersion { get; set; }
        public int AppliedVersion { get; set; }
        public OrchestrationData[] OrchestrationData { get; set; } = [];
        public SystemDataModel[] SystemData { get; set; } = [];
    }

    public class ContactChange : ChangeModel<Contact>
    {
        
    }
}

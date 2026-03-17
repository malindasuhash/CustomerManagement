using StateManagment.Models;

namespace Infrastructure
{
    internal partial class EntityCollectionConfig
    {
        public struct EntityMap
        {
            public static readonly EntityMap None = new EntityMap();
            public EntityName Name { get; set; }
            public string Collection { get; set; }

            public static EntityMap Create(EntityName name, string collectionName) 
            {
                return new EntityMap { Name = name, Collection = collectionName };
            }
        }
    }
}

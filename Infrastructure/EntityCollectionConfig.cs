using StateManagment.Entity;
using StateManagment.Models;

namespace Infrastructure
{
    internal class EntityCollectionConfig
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

        public static EntityMap Config<T>() where T : IEntity
        {
            if (typeof(T).IsAssignableFrom(typeof(Contact)))
            {
                return EntityMap.Create(EntityName.Contact, "contacts");
            }

            if (typeof(T).IsAssignableFrom(typeof(LegalEntity)))
            {
                return EntityMap.Create(EntityName.LegalEntity, "legal-entities");
            }

            if (typeof(T).IsAssignableFrom(typeof(BillingGroup)))
            {
                return EntityMap.Create(EntityName.BillingGroup, "billing-groups");
            }

            if (typeof(T).IsAssignableFrom(typeof(BankAccount)))
            {
                return EntityMap.Create(EntityName.BankAccount, "bank-accounts");
            }

            if (typeof(T).IsAssignableFrom(typeof(ProductAgreement)))
            {
                return EntityMap.Create(EntityName.ProductAgreement, "product-agreements");
            }

            if (typeof(T).IsAssignableFrom(typeof(TradingLocation)))
            {
                return EntityMap.Create(EntityName.TradingLocation, "trading-locations");
            }

            if (typeof(T).IsAssignableFrom(typeof(CustomerProfile)))
            {
                return EntityMap.Create(EntityName.CustomerProfile, "customer-profiles");
            }

            return EntityMap.None;
        }
    }
}

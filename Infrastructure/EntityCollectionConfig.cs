using StateManagment.Entity;
using StateManagment.Models;

namespace Infrastructure
{
    internal partial class EntityCollectionConfig
    {
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

            return EntityMap.None;
        }
    }
}

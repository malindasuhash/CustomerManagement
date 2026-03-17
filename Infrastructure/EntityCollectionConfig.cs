using StateManagment.Entity;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    internal partial class EntityCollectionConfig
    {
        public static string GetCollectionName(EntityName entityName)
        {
            return entityName switch
            {
                EntityName.Contact => "contacts",
                EntityName.LegalEntity => "legal-entities",
                EntityName.BillingGroup => "billing-groups",
                EntityName.BankAccount => "bank-accounts",
                EntityName.ProductAgreement => "product-agreements",
                _ => "none",
            };
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

            return EntityMap.None;
        }
    }
}

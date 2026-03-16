using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    internal class EntityNameToCollectionName
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
    }
}

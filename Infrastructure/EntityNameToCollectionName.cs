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
                _ => "none",
            };
        }
    }
}

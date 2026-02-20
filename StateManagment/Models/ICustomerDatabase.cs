using StateManagment.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public interface ICustomerDatabase
    {
        EntityBasics GetBasicInfo(EntityName contact, string entityId);

        void StoreDraft(EntityName entityName, IEntity entity, string entityId);

        void StoreSubmitted(EntityName entityName, IEntity entity, string entityId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Entity
{
    public class Contact : IEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public interface IEntity
    {   
    }
}

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

        override public string ToString()
        {
            return $"FirstName: {FirstName}, LastName: {LastName}";
        }
    }

    public interface IEntity
    {   
    }
}

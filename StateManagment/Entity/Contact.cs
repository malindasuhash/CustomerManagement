using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Entity
{
    public class Contact : IEntity
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Telephone { get; set; }
        public string TelephoneCode { get; set; }
        public string Email { get; set; }
        public string AltTelephone { get; set; }
        public string AltTelephoneCode { get; set; }
        public Address PostalAddress { get; set; }
        public MetaData[] Descriptors { get; set; } = [];
        public string Label { get; set; }
        override public string ToString()
        {
            return $"FirstName: {FirstName}, LastName: {LastName}";
        }
    }
}

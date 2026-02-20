using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Models
{
    public interface IRepository
    {
        void Add(MessageEnvelop messageEnvelop);
    }
}

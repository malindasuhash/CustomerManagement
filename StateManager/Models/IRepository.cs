using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager.Models
{
    public interface IRepository
    {
        void Add(MessageEnvelop messageEnvelop);
    }
}

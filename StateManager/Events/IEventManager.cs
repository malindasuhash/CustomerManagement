using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManager.Events
{
    public interface IEventManager
    {
        void Publish(IStateManagerEvent stateManagerEvent);
    }
}

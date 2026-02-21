using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemory
{
    public class DictionaryLock : IDistributedLock
    {
        private readonly ListDictionary locks = [];

        public Task<TaskOutcome> Lock(string key)
        {
            lock (locks.SyncRoot)
            {
                if (locks.Contains(key))
                {
                    return Task.FromResult(TaskOutcome.LOCK_UNAVAILABLE);
                }
                else
                {
                    locks.Add(key, true);
                    return Task.FromResult(TaskOutcome.OK);
                }
            }
        }

        public Task<TaskOutcome> Unlock(string key)
        {
            lock (locks.SyncRoot)
            {
                if (locks.Contains(key))
                {
                    locks.Remove(key);
                    return Task.FromResult(TaskOutcome.OK);
                }
                else
                {
                    return Task.FromResult(TaskOutcome.LOCK_UNAVAILABLE);
                }
            }
        }
    }
}

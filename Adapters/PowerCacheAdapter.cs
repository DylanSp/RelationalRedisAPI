using Adapters.Interfaces;
using Data;
using System;
using System.Collections.Generic;

namespace Adapters
{
    public class PowerCacheAdapter : IPowerAdapter
    {
        public IEnumerable<Power> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Power? Read(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Save(Power newData)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}

using Data;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Adapters
{
    public class PowerCacheAdapter : IEntityAdapter<Power>
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

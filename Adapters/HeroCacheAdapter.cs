using Adapters.Interfaces;
using Data;
using System;
using System.Collections.Generic;

namespace Adapters
{
    public class HeroCacheAdapter : IHeroAdapter
    {
        public IEnumerable<Hero> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Hero? Read(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Save(Hero newData)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}

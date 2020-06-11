using Data;
using Interfaces;
using System;
using System.Collections.Generic;

namespace Adapters
{
    public class TeamCacheAdapter : IEntityAdapter<Team>
    {
        public IEnumerable<Team> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Team? Read(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Save(Team newData)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}

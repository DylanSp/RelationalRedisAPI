using Adapters.Interfaces;
using Data;
using System;
using System.Collections.Generic;

namespace Adapters
{
    public class TeamCacheAdapter : ITeamAdapter
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

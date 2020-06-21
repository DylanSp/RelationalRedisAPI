using Data;
using Interfaces;
using System;
using System.Collections.Generic;

namespace Adapters.Interfaces
{
    public interface ITeamAdapter : IEntityAdapter<Team>
    {
        // TODO - tests for these in SQL/Redis adapters
        IEnumerable<Team> SearchTeams(string name);
        Team? SearchTeamsByMember(Guid heroId);
        Team? SearchTeamsByMember(string heroName);
    }
}
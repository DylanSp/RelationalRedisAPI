using Data;
using Interfaces;
using System.Collections.Generic;

namespace Adapters.Interfaces
{
    public interface IHeroAdapter : IEntityAdapter<Hero>
    {
        IEnumerable<Hero> SearchHeroes(string name, string location);   // TODO - search by power name and/or ID?
    }
}
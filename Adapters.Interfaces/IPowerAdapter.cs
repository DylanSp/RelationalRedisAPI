using Data;
using Interfaces;
using System.Collections.Generic;

namespace Adapters.Interfaces
{
    public interface IPowerAdapter : IEntityAdapter<Power>
    {
        // TODO - tests for this in SQL/Redis adapters
        IEnumerable<Power> SearchPowers(string name);
    }
}
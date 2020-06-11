using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data
{
    public struct Team : IEntity
    {
        public Guid Id { get; }
        public string Name { get; }
        public IEnumerable<Hero> Members { get; }

        public Team(Guid id, string name, IEnumerable<Hero> members)
        {
            this.Id = id;
            this.Name = name;
            this.Members = members;
        }

        // need to overload Equals() so we compare Members by value equality, instead of reference equality
        public override bool Equals(object obj)
        {
            if (!(obj is Team))
            {
                return false;
            }

            var team = (Team)obj;
            return Id.Equals(team.Id) &&
                   Name == team.Name &&
                   Members.SequenceEqual(team.Members);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Members);
        }
    }
}

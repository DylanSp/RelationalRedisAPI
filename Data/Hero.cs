using Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Data
{
    public struct Hero : IEntity
    {
        [Required]
        public Guid Id { get; }

        [Required]
        public string Name { get; }
        
        [Required]
        public string Location { get; }
        
        [Required]
        public IEnumerable<Power> Powers { get; }

        public Hero(Guid id, string name, string location, IEnumerable<Power> powers)
        {
            this.Id = id;
            this.Name = name;
            this.Location = location;
            this.Powers = powers;
        }

        // need to overload Equals() so we compare Members by value equality, instead of reference equality
        public override bool Equals(object obj)
        {
            if (!(obj is Hero))
            {
                return false;
            }

            var hero = (Hero)obj;
            return Id.Equals(hero.Id) &&
                   Name == hero.Name &&
                   Location == hero.Location &&
                   Powers.SequenceEqual(hero.Powers);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Location, Powers);
        }
    }
}

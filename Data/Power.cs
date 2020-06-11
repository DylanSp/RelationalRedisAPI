using Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data
{
    public struct Power : IEntity
    {
        public Guid Id { get; }
        public string Name { get; }

        public Power(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}

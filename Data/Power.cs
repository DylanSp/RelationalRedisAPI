using Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data
{
    public struct Power : IEntity
    {
        [Required]
        public Guid Id { get; }
        
        [Required]
        public string Name { get; }

        public Power(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using GraphQL.Types;

namespace RelationalRedisGraphQLAPI.GraphQL
{
    public class PowerInputType : InputObjectGraphType
    {
        public PowerInputType()
        {
            Name = "PowerInput";
            Field<StringGraphType>("name");
        }
    }
}

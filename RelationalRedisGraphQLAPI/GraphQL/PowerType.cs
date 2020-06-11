using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using GraphQL.Types;

namespace RelationalRedisGraphQLAPI.GraphQL
{
    public class PowerType : ObjectGraphType<Power>
    {
        public PowerType()
        {
            Field(power => power.Id, type: typeof(IdGraphType));
            Field(power => power.Name);
        }
    }
}

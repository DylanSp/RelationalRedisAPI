using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using GraphQL.Types;
using Interfaces;

namespace RelationalRedisGraphQLAPI.GraphQL
{
    public class SuperheroQuery : ObjectGraphType
    {
        public SuperheroQuery(IEntityAdapter<Power> powerAdapter)
        {
            Field<ListGraphType<PowerType>>(
                "powers",
                resolve: context => powerAdapter.ReadAll()
            );
        }
    }
}

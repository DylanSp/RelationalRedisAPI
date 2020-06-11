using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;

namespace RelationalRedisGraphQLAPI.GraphQL
{
    public class SuperheroSchema : Schema
    {
        public SuperheroSchema(IDependencyResolver resolver): base(resolver)
        {
            Query = resolver.Resolve<SuperheroQuery>();
            Mutation = resolver.Resolve<SuperheroMutation>();
        }
    }
}

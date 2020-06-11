using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;

namespace RelationalRedisGraphQLAPI.Controllers
{
    [Route("graphql")]
    public class GraphQLController : Controller
    {
        private ISchema Schema { get; }

        public GraphQLController(ISchema schema)
        {
            this.Schema = schema;
        }

        /*
        [HttpPost]
        public IActionResult Power([FromBody] GraphQLQuery query)
        */
    }
}

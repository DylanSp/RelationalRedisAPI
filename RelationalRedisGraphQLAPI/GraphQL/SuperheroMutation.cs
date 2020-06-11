using System;
using Data;
using GraphQL.Types;
using Interfaces;

namespace RelationalRedisGraphQLAPI.GraphQL
{
    public class SuperheroMutation : ObjectGraphType
    {
        public SuperheroMutation(IEntityAdapter<Power> powerAdapter)
        {
            Name = "CreatePowerMutation";
            Field<PowerType>(
                "createPower",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<PowerInputType>> {Name = "power"}
                ),
                resolve: context =>
                {
                    var powerName = context.GetArgument<string>("name");
                    var power = new Power(Guid.NewGuid(), powerName);
                    powerAdapter.Save(power);
                    return power;
                }
            );
        }
    }
}

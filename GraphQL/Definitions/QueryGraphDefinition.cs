using GraphQL.Resolvers;
using GraphQL.Types;

namespace HintKeep.GraphQL.Definitions;

public class QueryGraphDefinition : ObjectGraphType
{
    public QueryGraphDefinition()
    {
        AddField(new FieldType
        {
            Name = "field",
            Type = typeof(StringGraphType),
            Resolver = new FuncFieldResolver<string>(context => "Onions")
        });
    }
}
using GraphQL.Resolvers;
using GraphQL.Types;

namespace HintKeep.GraphQL.Definitions.UserAccounts;

[MutationField]
public class LogoutMutation : FieldType
{
    public LogoutMutation()
    {
        Name = "logout";
        Type = typeof(StringGraphType);
        Resolver = new FuncFieldResolver<string>(context =>
        {
            var userId = context.User.GetUserId();

            return userId.ToString("D");
        });
    }
}
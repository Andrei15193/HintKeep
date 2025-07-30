using GraphQL;
using GraphQL.Types;

namespace HintKeep.GraphQL.Definitions.UserAccounts;

[MutationField]
public class RegisterMutation : RequestFieldType<RegisterRequest, string>
{
    public RegisterMutation()
    {
        Name = "register";

        Arguments =
        [
            new QueryArgument<StringGraphType>
            {
                Name = nameof(RegisterRequest.Username)
            },
            new QueryArgument<StringGraphType>
            {
                Name = nameof(RegisterRequest.Password)
            }
        ];
        Type = typeof(StringGraphType);
    }

    protected override RegisterRequest GetInput(IResolveFieldContext context)
        => new(
            Username: context.GetArgument<string>(nameof(RegisterRequest.Username)),
            Password: context.GetArgument<string>(nameof(RegisterRequest.Password))
        );
}
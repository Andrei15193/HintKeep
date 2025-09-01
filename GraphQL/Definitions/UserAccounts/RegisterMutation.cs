using GraphQL;
using GraphQL.Types;

namespace HintKeep.GraphQL.Definitions.UserAccounts;

[MutationField(AllowAnonymous = true)]
public class RegisterMutation : RequestFieldType<RegisterRequest, RegisterResult>
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
            },
            new QueryArgument<StringGraphType>
            {
                Name = nameof(RegisterRequest.EmailAddress)
            }
        ];
        Type = typeof(AutoRegisteringObjectGraphType<RegisterResult>);
    }

    protected override RegisterRequest GetInput(IResolveFieldContext context)
        => new(
            Username: context.GetArgument<string>(nameof(RegisterRequest.Username)),
            Password: context.GetArgument<string>(nameof(RegisterRequest.Password)),
            EmailAddress: context.GetArgument<string>(nameof(RegisterRequest.EmailAddress))
        );
}
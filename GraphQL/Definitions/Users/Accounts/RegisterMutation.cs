using GraphQL;
using GraphQL.Types;

namespace HintKeep.GraphQL.Definitions.Users.Accounts;

[MutationField(AllowAnonymous = true)]
public class RegisterMutation : RequestFieldType<RegisterRequest, AuthenticationResult>
{
    public RegisterMutation()
    {
        Name = "Register";

        Arguments =
        [
            new QueryArgument<NonNullGraphType<StringGraphType>>
            {
                Name = nameof(RegisterRequest.Username)
            },
            new QueryArgument<NonNullGraphType<StringGraphType>>
            {
                Name = nameof(RegisterRequest.Password)
            },
            new QueryArgument<NonNullGraphType<StringGraphType>>
            {
                Name = nameof(RegisterRequest.Hint)
            },
            new QueryArgument<NonNullGraphType<StringGraphType>>
            {
                Name = nameof(RegisterRequest.EmailAddress)
            }
        ];
        Type = typeof(AuthenticationResultGraphType);
    }

    protected override RegisterRequest GetInput(IResolveFieldContext context)
        => new(
            Username: context.GetArgument<string>(nameof(RegisterRequest.Username)),
            Password: context.GetArgument<string>(nameof(RegisterRequest.Password)),
            Hint: context.GetArgument<string>(nameof(RegisterRequest.Hint)),
            EmailAddress: context.GetArgument<string>(nameof(RegisterRequest.EmailAddress))
        );
}
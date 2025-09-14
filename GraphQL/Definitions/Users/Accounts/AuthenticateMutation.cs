using GraphQL;
using GraphQL.Types;

namespace HintKeep.GraphQL.Definitions.Users.Accounts;

[MutationField(AllowAnonymous = true)]
public class AuthenticateMutation : RequestFieldType<AuthenticateRequest, AuthenticateResult>
{
    public AuthenticateMutation()
    {
        Name = "authenticate";

        Arguments =
        [
            new QueryArgument<StringGraphType>
            {
                Name = nameof(AuthenticateRequest.Username)
            },
            new QueryArgument<StringGraphType>
            {
                Name = nameof(AuthenticateRequest.Password)
            }
        ];
        Type = typeof(AuthenticateMutationResultGraphType);
    }

    protected override AuthenticateRequest GetInput(IResolveFieldContext context)
        => new(
            Username: context.GetArgument<string>(nameof(AuthenticateRequest.Username)),
            Password: context.GetArgument<string>(nameof(AuthenticateRequest.Password))
        );


    protected override async ValueTask<AuthenticateResult?> ResolveAsync(IResolveFieldContext context)
    {
        var result = await base.ResolveAsync(context);
        if (context.Errors.Count == 0 && result is not null)
        {
            context.SetHttpResponseCookie(
                HintKeepHttp.SessionTicketCookieName,
                result.SessionTicket,
                result.SessionTicketExpiration
            );
            context.SetHttpResponseCookie(
                HintKeepHttp.SessionTokenCookieName(result.SessionId),
                result.SessionToken,
                result.SessionTokenExpiration
            );
            context.SetDevHttpResponseCookie(
                HintKeepHttp.DevSessionTokenCookieName,
                result.SessionToken,
                result.SessionTokenExpiration
            );
        }

        return result;
    }

    private class AuthenticateMutationResultGraphType : ObjectGraphType<AuthenticateResult>
    {
        public AuthenticateMutationResultGraphType()
        {
            Field(x => x.UserId);
            Field(x => x.SessionId);
            Field(x => x.Username);
        }
    }
}
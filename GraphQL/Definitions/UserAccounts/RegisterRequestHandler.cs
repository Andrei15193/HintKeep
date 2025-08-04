using System.ComponentModel.DataAnnotations;

namespace HintKeep.GraphQL.Definitions.UserAccounts;

public record RegisterRequest(
    [property: MinLength(6, ErrorMessage = "Usernames must be at least 6 characters long.")]
    string Username,
    [property: Required(ErrorMessage = "A password is required.")]
    string Password
);

public class RegisterRequestHandler : IRequestHandler<RegisterRequest, string>
{
    public ValueTask<string> ExecuteAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (request.Username == "qwerty")
            throw new ValidationException(new ValidationResult("Duplicate value.", [nameof(request.Username)]), default, request.Username);
        else
            return ValueTask.FromResult(request.Username);
    }
}
using System.ComponentModel.DataAnnotations;
using Azure.Data.Tables;
using HintKeep.GraphQL.Data;

namespace HintKeep.GraphQL.Definitions.UserAccounts;

public record RegisterRequest(
    [property: MinLength(6, ErrorMessage = "Usernames must be at least 6 characters long.")]
    string Username,
    [property: Required(ErrorMessage = "A password is required."), RegularExpression("[A-Z][0-9]", ErrorMessage = "Password complexity not met.")]
    string Password
);

public class RegisterRequestHandler(HintKeepTableStorage hintKeepTableStorage) : IRequestHandler<RegisterRequest, string>
{
    public async ValueTask<string> ExecuteAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        await hintKeepTableStorage.Users.AddEntityAsync(new TableEntity
        {
            PartitionKey = request.Username,
            RowKey = request.Password
        });

        return request.Username;
    }
}
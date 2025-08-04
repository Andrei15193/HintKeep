namespace HintKeep.GraphQL.Data;

public class UserEntity
{
    public Guid Id { get; init; }

    public required string DisplayName { get; init; }

    public required string PasswordHash { get; init; }
}
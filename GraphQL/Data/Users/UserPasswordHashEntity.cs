using Azure;
using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data.Users;

public record struct UserPasswordHashEntity(
    string UsernameHash,
    string PasswordHash,
    Guid UserId,
    string Hint,
    ETag ETag = default
) :
    ITableEntityProvider
{
    public const string Type = "user-password-hash";
    public const string RowKeyPrefix = "password-hash:";
    public const string UserIdProperty = "userId";
    public const string HintProperty = "hint";

    public static TableEntityKey GetEntityKey(string usernameHash, string passwordHash)
        => new(usernameHash, RowKeyPrefix + passwordHash);

    public UserPasswordHashEntity(TableEntity tableEntity)
        : this(
            UsernameHash: tableEntity.GetString(nameof(TableEntity.PartitionKey)),
            PasswordHash: tableEntity.RowKey[RowKeyPrefix.Length..],
            UserId: tableEntity.GetGuid(UserIdProperty)!.Value,
            Hint: tableEntity.GetString(HintProperty)!,
            ETag: tableEntity.ETag
        )
        => TableEntityCommon.ValidateType(tableEntity, Type);

    public readonly TableEntity ToTableEntity()
        => new()
        {
            [TableEntityCommon.TypeProperty] = Type,

            PartitionKey = UsernameHash,
            RowKey = RowKeyPrefix + PasswordHash,
            ETag = ETag,

            [UserIdProperty] = UserId,
            [HintProperty] = Hint
        };
}

public static class UserPasswordHashEntityExtensions
{
    public static async ValueTask<UserPasswordHashEntity?> ToUserPasswordHashEntity(this Task<NullableResponse<TableEntity>> queryNullableResponse)
    {
        var result = await queryNullableResponse;
        if (result.HasValue)
            return new(result.Value!);
        else
            return default;
    }
}
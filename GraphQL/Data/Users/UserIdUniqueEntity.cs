using Azure;
using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data.Users;

public record struct UserIdUniqueEntity(Guid UserId, string UsernameHash, string Username, ETag ETag = default) : ITableEntityProvider
{
    public const string Type = "user-id-unique";
    public const string PartitionKey = "unique-id";
    public const string UsernameHashProperty = "usernameHash";
    public const string UsernameProperty = "username";

    public static TableEntityKey GetEntityKey(Guid UserId)
        => new(PartitionKey, UserId.ToString("D"));

    public UserIdUniqueEntity(TableEntity tableEntity)
        : this(
            UserId: Guid.ParseExact(tableEntity.GetString(nameof(TableEntity.RowKey)), "D"),
            UsernameHash: tableEntity.GetString(nameof(UsernameHashProperty)),
            Username: tableEntity.GetString(UsernameProperty)
        )
        => TableEntityCommon.ValidateType(tableEntity, Type);

    public readonly TableEntity ToTableEntity()
        => new()
        {
            [TableEntityCommon.TypeProperty] = Type,

            PartitionKey = PartitionKey,
            RowKey = UserId.ToString("D"),
            ETag = ETag,

            [UsernameHashProperty] = UsernameHash,
            [UsernameProperty] = Username
        };
}

public static class UserIdUniqueEntityExtensions
{
    public static async ValueTask<UserIdUniqueEntity?> ToUserIdUniqueEntity(this Task<NullableResponse<TableEntity>> queryNullableResponse)
    {
        var result = await queryNullableResponse;
        if (result.HasValue)
            return new(result.Value!);
        else
            return default;
    }
}
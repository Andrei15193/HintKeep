using Azure;
using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data.Users;

public record struct UserUniqueEntity(
    string UsernameHash,
    Guid UserId,
    ETag ETag = default
) :
    ITableEntityProvider
{
    public const string Type = "user-unique";
    public const string RowKey = "unique";
    public const string UserIdProperty = "userId";

    public UserUniqueEntity(TableEntity tableEntity)
        : this(
            UsernameHash: tableEntity.GetString(nameof(TableEntity.PartitionKey)),
            UserId: tableEntity.GetGuid(UserIdProperty)!.Value
        )
        => TableEntityCommon.ValidateType(tableEntity, Type);

    public readonly TableEntity ToTableEntity()
        => new()
        {
            [TableEntityCommon.TypeProperty] = Type,

            PartitionKey = UsernameHash,
            RowKey = RowKey,
            ETag = ETag,

            [UserIdProperty] = UserId
        };
}


public static class UserUniqueEntityExtensions
{
    public static async ValueTask<UserUniqueEntity?> ToUserUniqueEntity(this Task<NullableResponse<TableEntity>> queryNullableResponse)
    {
        var result = await queryNullableResponse;
        if (result.HasValue)
            return new(result.Value!);
        else
            return default;
    }
}
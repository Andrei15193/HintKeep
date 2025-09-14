using Azure;
using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data.Users;

public record struct UserSessionEntity(Guid UserId, Guid SessionId, ETag ETag = default) : ITableEntityProvider
{
    public const string Type = "user-session";
    public const string RowKeyPrefix = "session:";

    public UserSessionEntity(TableEntity tableEntity)
        : this(
            UserId: Guid.ParseExact(tableEntity.GetString(nameof(TableEntity.PartitionKey)), "D"),
            SessionId: Guid.ParseExact(tableEntity.GetString(nameof(TableEntity.RowKey))[RowKeyPrefix.Length..], "D")
        )
        => TableEntityCommon.ValidateType(tableEntity, Type);

    public readonly TableEntity ToTableEntity()
        => new()
        {
            [TableEntityCommon.TypeProperty] = Type,

            PartitionKey = UserId.ToString("D"),
            RowKey = RowKeyPrefix + SessionId.ToString("D"),
            ETag = ETag
        };
}

public static class UserSessionEntityExtensions
{
    public static async ValueTask<UserSessionEntity?> ToUserSessionEntity(this Task<NullableResponse<TableEntity>> queryNullableResponse)
    {
        var result = await queryNullableResponse;
        if (result.HasValue)
            return new(result.Value!);
        else
            return default;
    }
}
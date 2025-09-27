using Azure;
using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data.Users;

public record struct UserSessionTicketEntity(
    Guid UserId,
    Guid TicketId,
    DateTime TicketExpiration,
    ETag ETag = default
) :
    ITableEntityProvider
{
    public const string Type = "user-session-ticket";
    public const string RowKeyRefix = "session-ticket:";
    public const string UserIdProperty = "userId";
    public const string TokenIdProperty = "tokenId";
    public const string TokenExpirationProperty = "tokenExpiration";

    public static TableEntityKey GetEntityKey(Guid userId, Guid ticketId)
        => new(userId.ToString("D"), RowKeyRefix + ticketId);

    public UserSessionTicketEntity(TableEntity tableEntity)
        : this(
            UserId: Guid.ParseExact(tableEntity.GetString(nameof(TableEntity.PartitionKey)), "D"),
            TicketId: Guid.ParseExact(tableEntity.GetString(nameof(TableEntity.RowKey))[RowKeyRefix.Length..], "D"),
            TicketExpiration: tableEntity.GetDateTime(TokenExpirationProperty)!.Value,
            ETag: tableEntity.ETag
        )
        => TableEntityCommon.ValidateType(tableEntity, Type);

    public TableEntity ToTableEntity()
        => new()
        {
            [TableEntityCommon.TypeProperty] = Type,

            PartitionKey = UserId.ToString("D"),
            RowKey = RowKeyRefix + TicketId.ToString("D"),
            ETag = ETag,

            [UserIdProperty] = UserId,
            [TokenIdProperty] = TicketId,
            [TokenExpirationProperty] = TicketExpiration
        };
}

public static class UserSessionTicketEntityExtensions
{
    public static async ValueTask<UserSessionTicketEntity?> ToUserSessionTicketEntity(this Task<NullableResponse<TableEntity>> queryNullableResponse)
    {
        var result = await queryNullableResponse;
        if (result.HasValue)
            return new(result.Value!);
        else
            return default;
    }
}
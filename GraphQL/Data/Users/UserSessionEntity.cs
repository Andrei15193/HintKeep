using Azure;
using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data.Users;

public record struct UserSessionEntity(
    Guid UserId,
    Guid SessionId,
    Guid SessionTicketId,
    string RenewTicket,
    DateTime TokenExpiration,
    ETag ETag = default
) :
    ITableEntityProvider
{
    public const string Type = "user-session";
    public const string RowKeyPrefix = "session:";
    public const string SessionTicketIdTokenProperty = "sessionTicketId";
    public const string SessionRenewTicketProperty = "renewTicket";
    public const string TokenExpirationProperty = "tokenExpiration";

    public static TableEntityKey GetEntityKey(Guid userId, Guid sessionId)
        => new(userId.ToString("D"), RowKeyPrefix + sessionId.ToString("D"));

    public UserSessionEntity(TableEntity tableEntity)
        : this(
            UserId: Guid.ParseExact(tableEntity.GetString(nameof(TableEntity.PartitionKey)), "D"),
            SessionId: Guid.ParseExact(tableEntity.GetString(nameof(TableEntity.RowKey))[RowKeyPrefix.Length..], "D"),
            SessionTicketId: tableEntity.GetGuid(SessionTicketIdTokenProperty)!.Value,
            RenewTicket: tableEntity.GetString(SessionRenewTicketProperty),
            TokenExpiration: tableEntity.GetDateTime(TokenExpirationProperty)!.Value,
            ETag: tableEntity.ETag
        )
        => TableEntityCommon.ValidateType(tableEntity, Type);

    public readonly TableEntity ToTableEntity()
        => new()
        {
            [TableEntityCommon.TypeProperty] = Type,

            PartitionKey = UserId.ToString("D"),
            RowKey = RowKeyPrefix + SessionId.ToString("D"),
            ETag = ETag,

            [SessionTicketIdTokenProperty] = SessionTicketId,
            [SessionRenewTicketProperty] = RenewTicket,
            [TokenExpirationProperty] = TokenExpiration
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
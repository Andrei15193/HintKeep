using Azure;
using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data.Users;

public record struct UserEmailAddressHashEntity(string UsernameHash, string EmailAddressHash, ETag ETag = default) : ITableEntityProvider
{
    public const string Type = "user-email-address-hash";
    public const string RowKeyPrefix = "email-hash:";

    public UserEmailAddressHashEntity(TableEntity tableEntity)
        : this(
            UsernameHash: tableEntity.GetString(nameof(TableEntity.PartitionKey)),
            EmailAddressHash: tableEntity.RowKey[RowKeyPrefix.Length..],
            ETag: tableEntity.ETag
        )
        => TableEntityCommon.ValidateType(tableEntity, Type);

    public readonly TableEntity ToTableEntity()
        => new()
        {
            [TableEntityCommon.TypeProperty] = Type,

            PartitionKey = UsernameHash,
            RowKey = RowKeyPrefix + EmailAddressHash,
            ETag = ETag
        };
}

public static class UserEmailAddressHashEntityExtensions
{
    public static async ValueTask<UserEmailAddressHashEntity?> ToUserEmailAddressHashEntity(this Task<NullableResponse<TableEntity>> queryNullableResponse)
    {
        var result = await queryNullableResponse;
        if (result.HasValue)
            return new(result.Value!);
        else
            return default;
    }
}
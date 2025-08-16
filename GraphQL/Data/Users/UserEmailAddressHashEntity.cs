using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data.Users;

public record struct UserEmailAddressHashEntity(string Username, string EmailAddressHash) : ITableEntityProvider
{
    public const string RowKeyPrefix = "email-hash:";
    public const string Type = "user-email-address-hash";

    public UserEmailAddressHashEntity(TableEntity tableEntity)
        : this(
            Username: tableEntity.GetString(nameof(TableEntity.PartitionKey)),
            EmailAddressHash: tableEntity.RowKey[RowKeyPrefix.Length..]
        )
        => TableEntityCommon.ValidateType(tableEntity, Type);

    public readonly TableEntity ToTableEntity()
        => new()
        {
            { TableEntityCommon.TypeProperty, Type },
            { nameof(TableEntity.PartitionKey), Username.ToLowerInvariant() },
            { nameof(TableEntity.RowKey), RowKeyPrefix + EmailAddressHash }
        };
}

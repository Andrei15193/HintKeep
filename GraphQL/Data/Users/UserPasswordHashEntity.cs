using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data.Users;

public record struct UserPasswordHashEntity(string UsernameHash, string PasswordHash, Guid UserId, string DisplayName) : ITableEntityProvider
{
    public const string RowKeyPrefix = "password-hash:";
    public const string UserIdProperty = "displayName";
    public const string DisplayNameProperty = "displayName";
    public const string Type = "user-password-hash";

    public UserPasswordHashEntity(TableEntity tableEntity)
        : this(
            UsernameHash: tableEntity.GetString(nameof(TableEntity.PartitionKey)),
            PasswordHash: tableEntity.RowKey[RowKeyPrefix.Length..],
            UserId: tableEntity.GetGuid(UserIdProperty)!.Value,
            DisplayName: tableEntity.GetString(DisplayNameProperty)
        )
        => TableEntityCommon.ValidateType(tableEntity, Type);

    public readonly TableEntity ToTableEntity()
        => new()
        {
            { TableEntityCommon.TypeProperty, Type },
            { nameof(TableEntity.PartitionKey), UsernameHash },
            { nameof(TableEntity.RowKey), RowKeyPrefix + PasswordHash },
            { UserIdProperty, UserId },
            { DisplayNameProperty, DisplayName }
        };
}

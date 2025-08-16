using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data.Users;

public record struct UserIdUniqueEntity(Guid UserId, string UsernameHash) : ITableEntityProvider
{
    public const string PartitionKey = "unique-id";
    public const string UsernameHashProperty = "usernameHash";
    public const string Type = "user-id-unique";

    public UserIdUniqueEntity(TableEntity tableEntity)
        : this(
            UserId: Guid.ParseExact(tableEntity.GetString(nameof(TableEntity.PartitionKey)), "D"),
            UsernameHash: tableEntity.GetString(nameof(UsernameHashProperty))
        )
        => TableEntityCommon.ValidateType(tableEntity, Type);

    public readonly TableEntity ToTableEntity()
        => new()
        {
            { TableEntityCommon.TypeProperty, Type },
            { nameof(TableEntity.PartitionKey), PartitionKey },
            { nameof(TableEntity.RowKey), UserId.ToString("D") },
            { UsernameHashProperty, UsernameHash }
        };
}
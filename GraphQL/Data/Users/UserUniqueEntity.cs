using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data.Users;

public record struct UserUniqueEntity(string UsernameHash) : ITableEntityProvider
{
    public const string RowKey = "unique";
    public const string Type = "user-unique";

    public UserUniqueEntity(TableEntity tableEntity)
        : this(
            UsernameHash: tableEntity.GetString(nameof(TableEntity.PartitionKey))
        )
        => TableEntityCommon.ValidateType(tableEntity, Type);

    public readonly TableEntity ToTableEntity()
        => new()
        {
            { TableEntityCommon.TypeProperty, Type },
            { nameof(TableEntity.PartitionKey), UsernameHash },
            { nameof(TableEntity.RowKey), RowKey }
        };
}
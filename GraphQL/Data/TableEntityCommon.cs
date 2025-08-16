using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data;

public static class TableEntityCommon
{
    public const string TypeProperty = "type";

    public static void ValidateType(TableEntity tableEntity, string type)
    {
        var entityType = tableEntity.GetString(TypeProperty);
        if (entityType != type)
            throw new ArgumentException($"Expected entity of type '{type}' but got '{entityType}'.", nameof(tableEntity));
    }

    public static KeyValuePair<string, object> GetTableEntityType(string type)
        => new(TypeProperty, type);
}

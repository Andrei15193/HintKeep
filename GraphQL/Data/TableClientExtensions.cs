using Azure;
using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data;

public static class TableClientExtensions
{
    public static Task<NullableResponse<T>> GetEntityIfExistsAsync<T>(this TableClient tableClient, TableEntityKey tableKey, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
        where T : class, ITableEntity
        => tableClient.GetEntityIfExistsAsync<T>(tableKey.PartitionKey, tableKey.RowKey, select, cancellationToken);

    public static Task<NullableResponse<T>> GetEntityIfExistsAsync<T>(this TableClient tableClient, TableEntityKey tableKey, CancellationToken cancellationToken = default)
        where T : class, ITableEntity
        => tableClient.GetEntityIfExistsAsync<T>(tableKey.PartitionKey, tableKey.RowKey, default, cancellationToken);
}
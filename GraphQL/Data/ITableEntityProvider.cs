using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data;

public interface ITableEntityProvider
{
    public TableEntity ToTableEntity();
}

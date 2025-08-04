using Azure.Data.Tables;

namespace HintKeep.GraphQL.Data;

public class HintKeepTableStorage(TableServiceClient tableServiceClient)
{
    public TableClient Users { get; } = tableServiceClient.GetTableClient("Users");
}
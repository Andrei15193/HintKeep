using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage.Azure
{
    public class AzureEntityTables : IEntityTables
    {
        private readonly CloudTableClient _cloudTableClient;

        public AzureEntityTables(CloudTableClient cloudTableClient)
            => _cloudTableClient = cloudTableClient;

        public CloudTable Users
            => _cloudTableClient.GetTableReference(nameof(Users));
    }
}
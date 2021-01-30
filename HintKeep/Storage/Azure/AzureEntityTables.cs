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

        public CloudTable Logins
            => _cloudTableClient.GetTableReference(nameof(Logins));

        public CloudTable Accounts
            => _cloudTableClient.GetTableReference(nameof(Accounts));
    }
}
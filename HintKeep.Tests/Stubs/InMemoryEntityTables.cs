using CloudStub;
using HintKeep.Storage;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Tests.Stubs
{
    public class InMemoryEntityTables : IEntityTables
    {
        public CloudTable Users { get; } = new InMemoryCloudTable(nameof(Users));

        public CloudTable Logins { get; } = new InMemoryCloudTable(nameof(Logins));

        public CloudTable Accounts { get; } = new InMemoryCloudTable(nameof(Accounts));
    }
}
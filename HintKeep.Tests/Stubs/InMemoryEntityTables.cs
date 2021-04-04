using CloudStub;
using HintKeep.Storage;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Tests.Stubs
{
    public class InMemoryEntityTables : IEntityTables
    {
        public CloudTable Accounts { get; } = new InMemoryCloudTable(nameof(Accounts));
    }
}
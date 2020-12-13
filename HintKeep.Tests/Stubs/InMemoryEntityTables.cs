using CloudStub;
using HintKeep.Data;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Tests.Stubs
{
    public class InMemoryEntityTables : IEntityTables
    {
        public CloudTable Users { get; } = new InMemoryCloudTable(nameof(Users));
    }
}
using CloudStub;
using CloudStub.Core;
using CloudStub.Core.StorageHandlers;
using HintKeep.Storage;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Tests.Stubs
{
    public class InMemoryEntityTables : IEntityTables
    {
        private readonly ITableStorageHandler _tableStorageHandler = new InMemoryTableStorageHandler();

        public CloudTable Users
            => new StubCloudTable(new StubTable(nameof(Users), _tableStorageHandler));

        public CloudTable Accounts
            => new StubCloudTable(new StubTable(nameof(Accounts), _tableStorageHandler));

        public CloudTable AccountHints
            => new StubCloudTable(new StubTable(nameof(AccountHints), _tableStorageHandler));
    }
}
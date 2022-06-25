using CloudStub;
using CloudStub.Core;
using CloudStub.Core.StorageHandlers;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage.CloudStub
{
    public class CloudStubEntityTables : IEntityTables
    {
        private readonly ITableStorageHandler _tableStorageHandler;

        public CloudStubEntityTables(ITableStorageHandler tableStorageHandler)
            => _tableStorageHandler = tableStorageHandler;

        public CloudTable Users
            => new StubCloudTable(new StubTable(nameof(Users), _tableStorageHandler));

        public CloudTable Accounts
            => new StubCloudTable(new StubTable(nameof(Accounts), _tableStorageHandler));

        public CloudTable AccountHints
            => new StubCloudTable(new StubTable(nameof(AccountHints), _tableStorageHandler));
    }
}
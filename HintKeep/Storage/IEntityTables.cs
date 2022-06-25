using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage
{
    public interface IEntityTables
    {
        CloudTable Users { get; }

        CloudTable Accounts { get; }

        CloudTable AccountHints { get; }
    }
}
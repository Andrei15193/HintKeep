using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage
{
    public interface IEntityTables
    {
        CloudTable Users { get; }

        CloudTable Logins { get; }

        CloudTable Accounts { get; }
    }
}
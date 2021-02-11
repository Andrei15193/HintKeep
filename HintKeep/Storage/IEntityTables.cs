using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage
{
    public interface IEntityTables
    {
        CloudTable Logins { get; }

        CloudTable Users { get; }

        CloudTable UserSessions { get; }

        CloudTable Accounts { get; }
    }
}
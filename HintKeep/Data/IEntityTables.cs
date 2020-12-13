using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Data
{
    public interface IEntityTables
    {
        CloudTable Users { get; }
    }
}
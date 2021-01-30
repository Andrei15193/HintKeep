using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage.Entities
{
    public class IndexEntity : TableEntity
    {
        public string IndexedEntityId { get; set; }
    }
}
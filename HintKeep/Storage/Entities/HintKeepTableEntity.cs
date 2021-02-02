using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage.Entities
{
    public class HintKeepTableEntity : TableEntity
    {
        protected HintKeepTableEntity()
        {
        }

        public string EntityType { get; set; }
    }
}
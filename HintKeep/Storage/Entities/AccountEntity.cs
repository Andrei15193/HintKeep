using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage.Entities
{
    public class AccountEntity : TableEntity
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Hint { get; set; }

        public bool IsPinned { get; set; }
    }
}
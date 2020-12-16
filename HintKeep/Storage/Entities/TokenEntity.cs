using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage.Entities
{
    public class TokenEntity : TableEntity
    {
        public string Token { get; set; }

        public int Intent { get; set; }
    }
}
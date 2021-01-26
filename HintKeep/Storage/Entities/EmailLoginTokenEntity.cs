using System;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage.Entities
{
    public class EmailLoginTokenEntity : TableEntity
    {
        public string Token { get; set; }

        public DateTime Created { get; set; }
    }
}
using System;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage.Entities
{
    public class AccountHintEntity : TableEntity
    {
        public string AccountId { get; set; }

        public DateTime? StartDate { get; set; }

        public string Hint { get; set; }
    }
}
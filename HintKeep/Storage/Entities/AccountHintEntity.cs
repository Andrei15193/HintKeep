using System;

namespace HintKeep.Storage.Entities
{
    public class AccountHintEntity : HintKeepTableEntity
    {
        public string AccountId { get; set; }

        public string HintId { get; set; }

        public DateTime? DateAdded { get; set; }

        public string Hint { get; set; }
    }
}
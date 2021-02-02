using System;

namespace HintKeep.Storage.Entities
{
    public class EmailLoginTokenEntity : HintKeepTableEntity
    {
        public string Token { get; set; }

        public DateTime Created { get; set; }
    }
}
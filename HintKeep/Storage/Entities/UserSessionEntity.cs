using System;

namespace HintKeep.Storage.Entities
{
    public class UserSessionEntity : HintKeepTableEntity
    {
        public DateTime Expiration { get; set; }
    }
}
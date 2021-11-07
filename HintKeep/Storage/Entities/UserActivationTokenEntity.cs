using System;

namespace HintKeep.Storage.Entities
{
    public class UserActivationTokenEntity : HintKeepTableEntity
    {
        public DateTimeOffset Expiration { get; set; }
    }
}
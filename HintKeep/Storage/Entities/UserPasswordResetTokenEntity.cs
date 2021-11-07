using System;

namespace HintKeep.Storage.Entities
{
    public class UserPasswordResetTokenEntity : HintKeepTableEntity
    {
        public DateTimeOffset Expiration { get; set; }
    }
}
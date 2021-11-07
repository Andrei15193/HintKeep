using System;

namespace HintKeep.Storage.Entities
{
    public class UserEntity : HintKeepTableEntity
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public string Hint { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset? LastLoginTime { get; set; }
    }
}
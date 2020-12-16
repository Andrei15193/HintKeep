using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage.Entities
{
    public class UserEntity : TableEntity
    {
        public string Email { get; set; }

        public string PasswordSalt { get; set; }

        public string PasswordHash { get; set; }

        public int State { get; set; }
    }
}
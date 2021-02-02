namespace HintKeep.Storage.Entities
{
    public class EmailLoginEntity : HintKeepTableEntity
    {
        public string PasswordSalt { get; set; }

        public string PasswordHash { get; set; }

        public string State { get; set; }

        public string UserId { get; set; }
    }
}
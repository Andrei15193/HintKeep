namespace HintKeep.Storage.Entities
{
    public class UserEntity : HintKeepTableEntity
    {
        public string Email { get; set; }

        public bool IsDeleted { get; set; }
    }
}
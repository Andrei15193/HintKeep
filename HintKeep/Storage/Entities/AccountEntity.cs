namespace HintKeep.Storage.Entities
{
    public class AccountEntity : HintKeepTableEntity
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Hint { get; set; }

        public bool IsPinned { get; set; }

        public bool IsDeleted { get; set; }
    }
}
namespace HintKeep.ViewModels.Accounts
{
    public class AccountDetails
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Hint { get; set; }

        public string Notes { get; set; }

        public bool IsPinned { get; set; }

        public bool IsDeleted { get; set; }
    }
}
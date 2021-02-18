using System;

namespace HintKeep.Tests.Data
{
    public class AccountHint
    {
        public AccountHint()
        {
        }

        public AccountHint(AccountHint accountHint)
        {
            Id = accountHint.Id;
            Hint = accountHint.Hint;
            DateAdded = accountHint.DateAdded;
        }

        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Hint { get; set; } = "#Test-Hint";

        public DateTime? DateAdded { get; set; } = DateTime.UtcNow;
    }
}
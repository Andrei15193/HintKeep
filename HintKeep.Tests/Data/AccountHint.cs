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
            Hint = accountHint.Hint;
            StartDate = accountHint.StartDate;
        }

        public string Hint { get; set; } = "#Test-Hint";

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
    }
}
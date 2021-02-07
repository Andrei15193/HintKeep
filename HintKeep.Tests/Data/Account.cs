using System;
using System.Collections.Generic;
using System.Linq;

namespace HintKeep.Tests.Data
{
    public class Account
    {
        public Account()
        {
        }

        public Account(Account source)
        {
            UserId = source.UserId;
            Id = source.Id;
            Name = source.Name;
            Hints = source.Hints.Select(accountHint => new AccountHint(accountHint)).ToArray();
            Notes = source.Notes;
            IsPinned = source.IsPinned;
            IsDeleted = source.IsDeleted;
        }

        public string UserId { get; set; } = Guid.NewGuid().ToString("N");

        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Name { get; set; } = "Test-Name";

        public IReadOnlyCollection<AccountHint> Hints { get; set; } = new[] { new AccountHint() };

        public string Notes { get; set; } = "Test-Notes";

        public bool IsPinned { get; set; } = true;

        public bool IsDeleted { get; set; } = false;
    }
}
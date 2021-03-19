using System;
using System.Collections.Generic;
using HintKeep.ViewModels.Accounts;

namespace HintKeep.RequestsHandlers.Accounts
{
    public sealed class AccountSummarySortOrderComparer : IComparer<AccountSummary>
    {
        private const int LeftLesserThanRight = -1;
        private const int LeftEqualToRight = 0;
        private const int LeftGreaterThanRight = 1;

        public static int Compare(AccountSummary left, AccountSummary right)
        {
            if (left.IsPinned == right.IsPinned)
                return StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name);
            else if (left.IsPinned)
                return -1;
            else
                return 1;
        }

        int IComparer<AccountSummary>.Compare(AccountSummary left, AccountSummary right)
            => Compare(left, right);
    }
}
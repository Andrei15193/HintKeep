using System;
using System.Collections.Generic;
using HintKeep.Storage.Entities;

namespace HintKeep.Storage
{
    public class AccountHintEntitySortOrderComparer : IComparer<AccountHintEntity>
    {
        private const int LeftLesserThanRight = -1;
        private const int LeftEqualToRight = 0;
        private const int LeftGreaterThanRight = 1;

        public static int Compare(AccountHintEntity left, AccountHintEntity right)
        {
            if (left is null && right is null)
                return LeftEqualToRight;
            else if (left is null)
                return LeftGreaterThanRight;
            else if (right is null)
                return LeftLesserThanRight;
            if (left.DateAdded is null && right.DateAdded is null)
                return string.Compare(left.Hint, right.Hint, StringComparison.OrdinalIgnoreCase);
            else if (left.DateAdded is null)
                return LeftGreaterThanRight;
            else if (right.DateAdded is null)
                return LeftLesserThanRight;
            else
            {
                var dateAddedCompareResult = -left.DateAdded.Value.CompareTo(right.DateAdded.Value);
                if (dateAddedCompareResult == 0)
                    return string.Compare(left.Hint, right.Hint, StringComparison.OrdinalIgnoreCase);
                else
                    return dateAddedCompareResult;
            }
        }

        int IComparer<AccountHintEntity>.Compare(AccountHintEntity left, AccountHintEntity right)
            => Compare(left, right);
    }
}
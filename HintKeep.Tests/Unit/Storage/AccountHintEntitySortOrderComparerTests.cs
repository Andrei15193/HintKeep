using System;
using System.Collections.Generic;
using System.Globalization;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Xunit;

namespace HintKeep.Tests.Unit.Storage
{
    public class AccountHintEntitySortOrderComparerTests
    {
        private readonly IComparer<AccountHintEntity> _accountHintEntitySortOrderComparer = new AccountHintEntitySortOrderComparer();

        [Theory]
        [InlineData(null, null, null, null, 0)]
        [InlineData(null, null, "2020-02-20", null, 1)]
        [InlineData("2020-02-20", null, null, null, -1)]
        [InlineData("2020-02-20", null, "2020-02-20", null, 0)]
        [InlineData(null, "1", null, "1", 0)]
        [InlineData("2020-02-20", "1", "2020-02-20", "1", 0)]
        [InlineData("2020-02-20", "1", null, "1", -1)]
        [InlineData(null, "1", "2020-02-20", "1", 1)]
        [InlineData("2020-02-20", null, "2020-02-19", null, -1)]
        [InlineData("2020-02-19", null, "2020-02-20", null, 1)]
        [InlineData(null, "1", null, "2", -1)]
        [InlineData(null, "2", null, "1", 1)]
        [InlineData(null, null, null, "1", 1)]
        [InlineData(null, "1", null, null, -1)]
        public void Compare(string leftDateAdded, string leftHint, string rightDateAdded, string rightHint, int expectedResult)
            => Assert.Equal(expectedResult, _accountHintEntitySortOrderComparer.Compare(_GetAccountHint(leftDateAdded, leftHint), _GetAccountHint(rightDateAdded, rightHint)));

        private static AccountHintEntity _GetAccountHint(string dateAdded, string hint)
        {
            if (dateAdded is null && hint is null)
                return null;
            else if (dateAdded is null)
                return new AccountHintEntity
                {
                    Hint = hint
                };
            return new AccountHintEntity
            {
                DateAdded = DateTime.Parse(dateAdded, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                Hint = hint
            };
        }
    }
}
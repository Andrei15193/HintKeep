using System;
using System.Collections.Generic;
using System.Linq;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Data.Extensions
{
    public static class Accounts
    {
        public static IEntityTables AddAccounts(this IEntityTables entityTables, params Account[] accounts)
            => entityTables.AddAccounts((IEnumerable<Account>)accounts);

        public static IEntityTables AddAccounts(this IEntityTables entityTables, IEnumerable<Account> accounts)
        {
            foreach (var account in accounts)
            {
                var tableBatchOperation = new TableBatchOperation
                {
                    TableOperation.Insert(new IndexEntity
                    {
                        EntityType = "IndexEntity",
                        PartitionKey = account.UserId.ToEncodedKeyProperty(),
                        RowKey = $"name-{account.Name.ToLowerInvariant()}".ToEncodedKeyProperty(),
                        IndexedEntityId = account.Id,
                    }),
                    TableOperation.Insert(new AccountEntity
                    {
                        EntityType = "AccountEntity",
                        PartitionKey = account.UserId.ToEncodedKeyProperty(),
                        RowKey = $"id-{account.Id}".ToEncodedKeyProperty(),
                        Id = account.Id,
                        Name = account.Name,
                        Hint = account.Hints.OrderByDescending(accountHint => accountHint.DateAdded).First().Hint,
                        Notes = account.Notes,
                        IsPinned = account.IsPinned,
                        IsDeleted = account.IsDeleted
                    })
                };
                foreach (var accountHint in account.Hints)
                    tableBatchOperation.Add(TableOperation.Insert(new AccountHintEntity
                    {
                        EntityType = "AccountHintEntity",
                        PartitionKey = account.UserId.ToEncodedKeyProperty(),
                        RowKey = $"id-{account.Id}-hintId-{accountHint.Id}".ToEncodedKeyProperty(),
                        AccountId = account.Id,
                        HintId = accountHint.Id,
                        Hint = accountHint.Hint,
                        DateAdded = accountHint.DateAdded
                    }));
                entityTables.Accounts.ExecuteBatch(tableBatchOperation);
            }

            return entityTables;
        }

        public static IEntityTables AssertAccounts(this IEntityTables entityTables, params Account[] accounts)
            => entityTables.AssertAccounts((IEnumerable<Account>)accounts);

        public static IEntityTables AssertAccounts(this IEntityTables entityTables, IEnumerable<Account> accounts)
        {
            foreach (var account in accounts)
                Assert.NotNull(account.UserId);

            Assert.Equal(accounts.Sum(account => 2 + account.Hints.Count), entityTables.Accounts.ExecuteQuery(new TableQuery()).Count());
            foreach (var account in accounts)
            {
                var indexEntity = (IndexEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>(account.UserId.ToEncodedKeyProperty(), $"name-{account.Name.ToLowerInvariant()}".ToEncodedKeyProperty())).Result;
                Assert.NotNull(indexEntity);
                Assert.Equal("IndexEntity", indexEntity.EntityType);
                Assert.Equal(account.UserId, indexEntity.PartitionKey.FromEncodedKeyProperty());
                Assert.Equal($"name-{account.Name.ToLowerInvariant()}", indexEntity.RowKey.FromEncodedKeyProperty());
                Assert.Equal(account.Id, indexEntity.IndexedEntityId);

                var accountEntity = (AccountEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>(account.UserId.ToEncodedKeyProperty(), $"id-{account.Id}".ToEncodedKeyProperty())).Result;
                Assert.NotNull(accountEntity);
                Assert.Equal("AccountEntity", accountEntity.EntityType);
                Assert.Equal(account.UserId, accountEntity.PartitionKey.FromEncodedKeyProperty());
                Assert.Equal($"id-{account.Id}", accountEntity.RowKey.FromEncodedKeyProperty());
                Assert.Equal(account.Id, accountEntity.Id);
                Assert.Equal(account.Name, accountEntity.Name);
                Assert.Equal(account.Hints.OrderByDescending(accountHint => accountHint.DateAdded).First().Hint, accountEntity.Hint);
                Assert.Equal(account.Notes, accountEntity.Notes);

                if (account.IsPinned)
                    Assert.True(accountEntity.IsPinned);
                else
                    Assert.False(accountEntity.IsPinned);

                if (account.IsDeleted)
                    Assert.True(accountEntity.IsDeleted);
                else
                    Assert.False(accountEntity.IsDeleted);

                foreach (var accountHint in account.Hints)
                {
                    var accountHintEntity = (AccountHintEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>(account.UserId.ToEncodedKeyProperty(), $"id-{account.Id}-hintId-{accountHint.Id}".ToEncodedKeyProperty())).Result;
                    Assert.NotNull(accountHintEntity);
                    Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
                    Assert.Equal(account.UserId, accountHintEntity.PartitionKey.FromEncodedKeyProperty());
                    Assert.Equal($"id-{account.Id}-hintId-{accountHint.Id}", accountHintEntity.RowKey.FromEncodedKeyProperty());
                    Assert.Equal(account.Id, accountHintEntity.AccountId);
                    Assert.Equal(accountHint.Id, accountHintEntity.HintId);
                    Assert.Equal(accountHint.Hint, accountHintEntity.Hint);
                    Assert.Equal(accountHint.DateAdded, accountHintEntity.DateAdded);
                }
            }

            return entityTables;
        }
    }
}
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
                entityTables.Accounts.ExecuteBatch(new TableBatchOperation
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
                        RowKey = $"accountId-{account.Id}".ToEncodedKeyProperty(),
                        Id = account.Id,
                        Name = account.Name,
                        Hint = account.LatestHint,
                        Notes = account.Notes,
                        IsPinned = account.IsPinned,
                        IsDeleted = account.IsDeleted
                    })
                });

                var tableBatchOperation = account.Hints.Aggregate(
                    new TableBatchOperation(),
                    (tableBatchOperation, accountHint) =>
                    {
                        tableBatchOperation.Add(TableOperation.Insert(new AccountHintEntity
                        {
                            EntityType = "AccountHintEntity",
                            PartitionKey = $"accountId-{account.Id}".ToEncodedKeyProperty(),
                            RowKey = $"hintId-{accountHint.Id}".ToEncodedKeyProperty(),
                            AccountId = account.Id,
                            HintId = accountHint.Id,
                            Hint = accountHint.Hint,
                            DateAdded = accountHint.DateAdded
                        }));
                        return tableBatchOperation;
                    }
                );
                if (tableBatchOperation.Count > 0)
                    entityTables.AccountHints.ExecuteBatch(tableBatchOperation);
            }

            return entityTables;
        }

        public static IEntityTables AssertAccounts(this IEntityTables entityTables, params Account[] accounts)
            => entityTables.AssertAccounts((IEnumerable<Account>)accounts);

        public static IEntityTables AssertAccounts(this IEntityTables entityTables, IEnumerable<Account> accounts)
        {
            foreach (var account in accounts)
                Assert.NotNull(account.UserId);

            Assert.Equal(2 * accounts.Count(), entityTables.Accounts.ExecuteQuery(new TableQuery()).Count());
            Assert.Equal(accounts.Sum(account => account.Hints.Count), entityTables.AccountHints.ExecuteQuery(new TableQuery()).Count());
            foreach (var account in accounts)
            {
                var indexEntity = (IndexEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>(account.UserId.ToEncodedKeyProperty(), $"name-{account.Name.ToLowerInvariant()}".ToEncodedKeyProperty())).Result;
                Assert.NotNull(indexEntity);
                Assert.Equal("IndexEntity", indexEntity.EntityType);
                Assert.Equal(account.UserId.ToEncodedKeyProperty(), indexEntity.PartitionKey);
                Assert.Equal($"name-{account.Name.ToLowerInvariant()}".ToEncodedKeyProperty(), indexEntity.RowKey);
                Assert.Equal(account.Id, indexEntity.IndexedEntityId);

                var accountEntity = (AccountEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>(account.UserId.ToEncodedKeyProperty(), $"accountId-{account.Id}".ToEncodedKeyProperty())).Result;
                Assert.NotNull(accountEntity);
                Assert.Equal("AccountEntity", accountEntity.EntityType);
                Assert.Equal(account.UserId.ToEncodedKeyProperty(), accountEntity.PartitionKey);
                Assert.Equal($"accountId-{account.Id}".ToEncodedKeyProperty(), accountEntity.RowKey);
                Assert.Equal(account.Id, accountEntity.Id);
                Assert.Equal(account.Name, accountEntity.Name);
                Assert.Equal(account.LatestHint, accountEntity.Hint);
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
                    var accountHintEntity = (AccountHintEntity)entityTables.AccountHints.Execute(TableOperation.Retrieve<AccountHintEntity>($"accountId-{account.Id}".ToEncodedKeyProperty(), $"hintId-{accountHint.Id}".ToEncodedKeyProperty())).Result;
                    Assert.NotNull(accountHintEntity);
                    Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
                    Assert.Equal($"accountId-{account.Id}".ToEncodedKeyProperty(), accountHintEntity.PartitionKey);
                    Assert.Equal($"hintId-{accountHint.Id}".ToEncodedKeyProperty(), accountHintEntity.RowKey);
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
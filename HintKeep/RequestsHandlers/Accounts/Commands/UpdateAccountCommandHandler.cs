using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Accounts.Commands;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.Accounts.Commands
{
    public class UpdateAccountCommandHandler : AsyncRequestHandler<UpdateAccountCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _login;

        public UpdateAccountCommandHandler(IEntityTables entityTables, Session login)
            => (_entityTables, _login) = (entityTables, login);

        protected override async Task Handle(UpdateAccountCommand command, CancellationToken cancellationToken)
        {
            var accountEntity = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountEntity>(
                    _login.UserId.ToEncodedKeyProperty(),
                    $"id-{command.Id}".ToEncodedKeyProperty(),
                    new List<string>
                    {
                        nameof(AccountEntity.Name),
                        nameof(AccountEntity.Hint),
                        nameof(AccountEntity.IsDeleted)
                    }
                ),
                cancellationToken
            )).Result;
            if (accountEntity is null || accountEntity.IsDeleted)
                throw new NotFoundException();

            var tableBatchOperation = new TableBatchOperation();
            if (!string.Equals(command.Name, accountEntity.Name, StringComparison.OrdinalIgnoreCase))
            {
                var indexEntity = (IndexEntity)(await _entityTables.Accounts.ExecuteAsync(
                    TableOperation.Retrieve<IndexEntity>(
                        _login.UserId.ToEncodedKeyProperty(),
                        $"name-{accountEntity.Name.ToLowerInvariant()}".ToEncodedKeyProperty(),
                        new List<string>()
                    ),
                    cancellationToken
                )).Result;
                tableBatchOperation.Add(TableOperation.Delete(indexEntity));
                tableBatchOperation.Add(TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = _login.UserId.ToEncodedKeyProperty(),
                    RowKey = $"name-{command.Name.ToLowerInvariant()}".ToEncodedKeyProperty(),
                    IndexedEntityId = command.Id
                }));
            }

            if (!string.Equals(command.Hint, accountEntity.Hint, StringComparison.Ordinal))
            {
                var now = DateTime.UtcNow;
                var hintId = Guid.NewGuid().ToString("N");
                tableBatchOperation.Add(TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = _login.UserId.ToEncodedKeyProperty(),
                    RowKey = $"id-{command.Id}-hintId-{hintId}".ToEncodedKeyProperty(),
                    AccountId = command.Id,
                    HintId = hintId,
                    Hint = command.Hint,
                    DateAdded = now,
                }));
            }
            accountEntity.Name = command.Name;
            accountEntity.Hint = command.Hint;
            accountEntity.Notes = command.Notes;
            accountEntity.IsPinned = command.IsPinned;

            tableBatchOperation.Add(TableOperation.Merge(accountEntity));

            await _entityTables.Accounts.ExecuteBatchAsync(tableBatchOperation, cancellationToken);
        }
    }
}
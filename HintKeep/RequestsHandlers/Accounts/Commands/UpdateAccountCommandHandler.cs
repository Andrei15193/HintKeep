using System;
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
        private readonly LoginInfo _login;

        public UpdateAccountCommandHandler(IEntityTables entityTables, LoginInfo login)
            => (_entityTables, _login) = (entityTables, login);

        protected override async Task Handle(UpdateAccountCommand command, CancellationToken cancellationToken)
        {
            var accountEntity = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(TableOperation.Retrieve<AccountEntity>(_login.UserId, $"id-{command.Id}"), cancellationToken)).Result;
            if (accountEntity is null || accountEntity.IsDeleted)
                throw new NotFoundException();

            var tableBatchOperation = new TableBatchOperation();

            if (!string.Equals(command.Name, accountEntity.Name, StringComparison.OrdinalIgnoreCase))
            {
                var indexEntity = (IndexEntity)(await _entityTables.Accounts.ExecuteAsync(TableOperation.Retrieve<IndexEntity>(_login.UserId, $"name-{accountEntity.Name.ToLowerInvariant()}"), cancellationToken)).Result;
                tableBatchOperation.Add(TableOperation.Delete(indexEntity));
                tableBatchOperation.Add(TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = _login.UserId,
                    RowKey = $"name-{command.Name.ToLowerInvariant()}",
                    IndexedEntityId = command.Id
                }));
            }

            if (!string.Equals(command.Hint, accountEntity.Hint, StringComparison.Ordinal))
            {
                var now = DateTime.UtcNow;
                tableBatchOperation.Add(TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = _login.UserId,
                    RowKey = $"id-{command.Id}-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}",
                    Hint = command.Hint,
                    AccountId = command.Id,
                    StartDate = now,
                }));
            }
            accountEntity.Name = command.Name;
            accountEntity.Hint = command.Hint;
            accountEntity.IsPinned = command.IsPinned;

            tableBatchOperation.Add(TableOperation.Replace(accountEntity));

            await _entityTables.Accounts.ExecuteBatchAsync(tableBatchOperation, cancellationToken);
        }
    }
}
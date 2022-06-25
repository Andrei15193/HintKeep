using System;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.AccountsHints.Commands;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.AccountsHints.Commands
{
    public class AddAccountHintCommandHandler : IRequestHandler<AddAccountHintCommand, string>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _session;

        public AddAccountHintCommandHandler(IEntityTables entityTables, Session session)
            => (_entityTables, _session) = (entityTables, session);

        public async Task<string> Handle(AddAccountHintCommand command, CancellationToken cancellationToken)
        {
            var accountEntity = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountEntity>(_session.UserId.ToEncodedKeyProperty(), $"accountId-{command.AccountId}".ToEncodedKeyProperty()),
                cancellationToken
            )).Result;
            if (accountEntity is null || accountEntity.IsDeleted)
                throw new NotFoundException();

            accountEntity.Hint = null;
            await _entityTables.Accounts.ExecuteAsync(TableOperation.Replace(accountEntity), cancellationToken);

            var hintId = Guid.NewGuid().ToString("N");
            var accountHintEntity = new AccountHintEntity
            {
                EntityType = "AccountHintEntity",
                PartitionKey = $"accountId-{command.AccountId}".ToEncodedKeyProperty(),
                RowKey = $"hintId-{hintId}".ToEncodedKeyProperty(),
                AccountId = command.AccountId,
                HintId = hintId,
                Hint = command.Hint,
                DateAdded = command.DateAdded
            };
            await _entityTables.AccountHints.ExecuteAsync(TableOperation.Insert(accountHintEntity), cancellationToken);
            return hintId;
        }
    }
}
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
    public class UpdateAccountHintCommandHandler : AsyncRequestHandler<UpdateAccountHintCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _session;

        public UpdateAccountHintCommandHandler(IEntityTables entityTables, Session session)
            => (_entityTables, _session) = (entityTables, session);

        protected override async Task Handle(UpdateAccountHintCommand command, CancellationToken cancellationToken)
        {
            var accountEntity = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountEntity>(_session.UserId.ToEncodedKeyProperty(), $"accountId-{command.AccountId}".ToEncodedKeyProperty()),
                cancellationToken
            )).Result;
            if (accountEntity is null || accountEntity.IsDeleted)
                throw new NotFoundException();

            var accountHintEntity = (AccountHintEntity)(await _entityTables.AccountHints.ExecuteAsync(
                TableOperation.Retrieve<AccountHintEntity>($"accountId-{command.AccountId}".ToEncodedKeyProperty(), $"hintId-{command.HintId}".ToEncodedKeyProperty()),
                cancellationToken
            )).Result;

            if (accountHintEntity is null)
                throw new NotFoundException();

            accountEntity.Hint = null;
            await _entityTables.Accounts.ExecuteAsync(TableOperation.Replace(accountEntity), cancellationToken);

            accountHintEntity.DateAdded = command.DateAdded;
            await _entityTables.AccountHints.ExecuteAsync(TableOperation.Replace(accountHintEntity), cancellationToken);
        }
    }
}
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
    public class MoveAccountToBinCommandHandler : AsyncRequestHandler<MoveAccountToBinCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly LoginInfo _login;

        public MoveAccountToBinCommandHandler(IEntityTables entityTables, LoginInfo login)
            => (_entityTables, _login) = (entityTables, login);

        protected override async Task Handle(MoveAccountToBinCommand command, CancellationToken cancellationToken)
        {
            var accountEntity = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(TableOperation.Retrieve<AccountEntity>(_login.UserId, $"id-{command.Id}"), cancellationToken)).Result;
            if (accountEntity is null || accountEntity.IsDeleted)
                throw new NotFoundException();

            accountEntity.IsDeleted = true;
            await _entityTables.Accounts.ExecuteAsync(TableOperation.Replace(accountEntity), cancellationToken);
        }
    }
}
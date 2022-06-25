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
    public class UpdateDeletedAccountCommandHandler : AsyncRequestHandler<UpdateDeletedAccountCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _login;

        public UpdateDeletedAccountCommandHandler(IEntityTables entityTables, Session login)
            => (_entityTables, _login) = (entityTables, login);

        protected override async Task Handle(UpdateDeletedAccountCommand command, CancellationToken cancellationToken)
        {
            var accountEntity = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountEntity>(
                    _login.UserId.ToEncodedKeyProperty(),
                    $"accountId-{command.Id}".ToEncodedKeyProperty(),
                    new List<string> { nameof(AccountEntity.IsDeleted) }
                ),
                cancellationToken
            )).Result;
            if (accountEntity is null || !accountEntity.IsDeleted)
                throw new NotFoundException();

            await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Merge(
                    new DynamicTableEntity
                    {
                        PartitionKey = accountEntity.PartitionKey,
                        RowKey = accountEntity.RowKey,
                        ETag = accountEntity.ETag,
                        Properties =
                        {
                            { nameof(AccountEntity.IsDeleted), EntityProperty.GeneratePropertyForBool(command.IsDeleted) }
                        }
                    }
                ),
                cancellationToken
                );
        }
    }
}
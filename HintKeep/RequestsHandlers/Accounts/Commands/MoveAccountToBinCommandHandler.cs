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
    public class MoveAccountToBinCommandHandler : AsyncRequestHandler<MoveAccountToBinCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _login;

        public MoveAccountToBinCommandHandler(IEntityTables entityTables, Session login)
            => (_entityTables, _login) = (entityTables, login);

        protected override async Task Handle(MoveAccountToBinCommand command, CancellationToken cancellationToken)
        {
            var accountEntity = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountEntity>(
                    _login.UserId.ToEncodedKeyProperty(),
                    $"id-{command.Id}".ToEncodedKeyProperty(),
                    new List<string>
                    {
                        nameof(AccountEntity.IsDeleted)
                    }
                ),
                cancellationToken
            )).Result;
            if (accountEntity is null || accountEntity.IsDeleted)
                throw new NotFoundException();

            await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Merge(
                    new DynamicTableEntity
                    {
                        PartitionKey = accountEntity.PartitionKey,
                        RowKey = accountEntity.RowKey,
                        ETag = accountEntity.ETag,
                        Properties = new Dictionary<string, EntityProperty>
                        {
                            { nameof(AccountEntity.IsDeleted), EntityProperty.GeneratePropertyForBool(true) }
                        }
                    }
                ),
                cancellationToken
            );
        }
    }
}
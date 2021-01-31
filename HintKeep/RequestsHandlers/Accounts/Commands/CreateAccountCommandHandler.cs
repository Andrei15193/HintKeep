using System;
using System.Net;
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
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, string>
    {
        private readonly IEntityTables _entityTables;
        private readonly LoginInfo _login;

        public CreateAccountCommandHandler(IEntityTables entityTables, LoginInfo login)
            => (_entityTables, _login) = (entityTables, login);

        public async Task<string> Handle(CreateAccountCommand command, CancellationToken cancellationToken)
        {
            var accountId = Guid.NewGuid().ToString("N");
            try
            {
                await _entityTables.Accounts.ExecuteBatchAsync(
                    new TableBatchOperation{
                    TableOperation.Insert(
                        new IndexEntity
                        {
                            PartitionKey = $"{_login.UserId}",
                            RowKey = $"name-{command.Name.ToLowerInvariant()}",
                            IndexedEntityId = accountId
                        }
                    ),
                    TableOperation.Insert(
                        new AccountEntity
                        {
                            PartitionKey = $"{_login.UserId}",
                            RowKey = $"id-{accountId}",
                            Name = command.Name,
                            Hint = command.Hint,
                            IsPinned = command.IsPinned
                        }
                    )
                    },
                    cancellationToken
                );
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                throw new ConflictException(storageException);
            }

            return accountId;
        }
    }
}
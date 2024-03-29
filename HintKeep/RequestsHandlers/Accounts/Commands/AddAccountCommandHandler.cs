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
    public class AddAccountCommandHandler : IRequestHandler<AddAccountCommand, string>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _login;

        public AddAccountCommandHandler(IEntityTables entityTables, Session login)
            => (_entityTables, _login) = (entityTables, login);

        public async Task<string> Handle(AddAccountCommand command, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var accountId = Guid.NewGuid().ToString("N");
            var hintId = Guid.NewGuid().ToString("N");
            try
            {
                await _entityTables.Accounts.ExecuteBatchAsync(
                    new TableBatchOperation
                    {
                        TableOperation.Insert(
                            new IndexEntity
                            {
                                EntityType = "IndexEntity",
                                PartitionKey = _login.UserId.ToEncodedKeyProperty(),
                                RowKey = $"name-{command.Name.ToLowerInvariant()}".ToEncodedKeyProperty(),
                                IndexedEntityId = accountId
                            }
                        ),
                        TableOperation.Insert(
                            new AccountEntity
                            {
                                EntityType = "AccountEntity",
                                PartitionKey = _login.UserId.ToEncodedKeyProperty(),
                                RowKey = $"accountId-{accountId}".ToEncodedKeyProperty(),
                                Id = accountId,
                                Name = command.Name,
                                Hint = command.Hint,
                                Notes = command.Notes,
                                IsPinned = command.IsPinned,
                                IsDeleted = false
                            }
                        )
                    },
                    cancellationToken
                );

                await _entityTables.AccountHints.ExecuteAsync(
                    TableOperation.Insert(
                        new AccountHintEntity
                        {
                            EntityType = "AccountHintEntity",
                            PartitionKey =  $"accountId-{accountId}".ToEncodedKeyProperty(),
                            RowKey = $"hintId-{hintId}".ToEncodedKeyProperty(),
                            AccountId = accountId,
                            HintId = hintId,
                            Hint = command.Hint,
                            DateAdded = now
                        }
                    )
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
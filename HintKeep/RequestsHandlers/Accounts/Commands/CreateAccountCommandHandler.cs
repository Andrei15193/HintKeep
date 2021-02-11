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
        private readonly Session _login;

        public CreateAccountCommandHandler(IEntityTables entityTables, Session login)
            => (_entityTables, _login) = (entityTables, login);

        public async Task<string> Handle(CreateAccountCommand command, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var accountId = Guid.NewGuid().ToString("N");
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
                                RowKey = $"id-{accountId}".ToEncodedKeyProperty(),
                                Id = accountId,
                                Name = command.Name,
                                Hint = command.Hint,
                                Notes = command.Notes,
                                IsPinned = command.IsPinned,
                                IsDeleted = false
                            }
                        ),
                        TableOperation.Insert(
                            new AccountHintEntity
                            {
                                EntityType = "AccountHintEntity",
                                PartitionKey = _login.UserId.ToEncodedKeyProperty(),
                                RowKey = $"id-{accountId}-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}".ToEncodedKeyProperty(),
                                AccountId = accountId,
                                StartDate = now,
                                Hint = command.Hint
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
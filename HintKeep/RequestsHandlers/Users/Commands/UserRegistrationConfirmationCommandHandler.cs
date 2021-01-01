using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Users.Commands;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.Users.Commands
{
    public class UserRegistrationConfirmationCommandHandler : AsyncRequestHandler<UserRegistrationConfirmationCommand>
    {
        private readonly IEntityTables _entityTables;

        public UserRegistrationConfirmationCommandHandler(IEntityTables entityTables)
            => _entityTables = entityTables;

        protected override async Task Handle(UserRegistrationConfirmationCommand command, CancellationToken cancellationToken)
        {
            var queryResult = await _entityTables.Users.ExecuteQuerySegmentedAsync(new TableQuery()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, command.Email.ToLowerInvariant()),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.Equal, "user")
                        ),
                        TableOperators.Or,
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, command.Email.ToLowerInvariant()),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.Equal, "confirmation_tokens-" + command.ConfirmationToken.ToLowerInvariant())
                        )
                    )
                )
                .Select(new[]
                {
                    nameof(ITableEntity.PartitionKey),
                    nameof(ITableEntity.RowKey),
                    nameof(ITableEntity.ETag),
                    nameof(UserEntity.State),
                    nameof(TokenEntity.Token),
                    nameof(TokenEntity.Intent),
                    nameof(TokenEntity.Created)
                })
                .Take(2),
                null,
                cancellationToken
            );

            var userEntity = queryResult.SingleOrDefault(entity => entity.RowKey == "user");
            var tokenEntity = queryResult.SingleOrDefault(entity => entity.RowKey.StartsWith("confirmation_tokens-"));
            if (userEntity is object
                && tokenEntity is object
                && userEntity.Properties[nameof(UserEntity.State)].Int32Value == (int)UserState.PendingConfirmation
                && tokenEntity.Properties[nameof(TokenEntity.Intent)].Int32Value == (int)TokenIntent.ConfirmUserRegistration
                && (DateTime.UtcNow - tokenEntity.Properties[nameof(TokenEntity.Created)].DateTime.Value).TotalDays < 1)
            {
                userEntity.Properties[nameof(UserEntity.State)].Int32Value = (int)UserState.Confirmed;

                await _entityTables.Users.ExecuteBatchAsync(
                    new TableBatchOperation { TableOperation.Merge(userEntity), TableOperation.Delete(tokenEntity) },
                    cancellationToken
                );
            }
            else
                throw new PreconditionFailedException();
        }
    }
}
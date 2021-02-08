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
            var queryResult = await _entityTables.Logins.ExecuteQuerySegmentedAsync(new TableQuery()
                .Where(TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, command.Email.ToLowerInvariant().ToEncodedKeyProperty()))
                .Select(new[]
                {
                    nameof(ITableEntity.PartitionKey),
                    nameof(ITableEntity.RowKey),
                    nameof(ITableEntity.ETag),
                    nameof(HintKeepTableEntity.EntityType),
                    nameof(EmailLoginEntity.State),
                    nameof(EmailLoginTokenEntity.Token),
                    nameof(EmailLoginTokenEntity.Created)
                })
                .Take(2),
                null,
                cancellationToken
            );

            var loginEntity = queryResult.SingleOrDefault(entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "EmailLoginEntity");
            var tokenEntity = queryResult.SingleOrDefault(entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "EmailLoginTokenEntity");
            if (loginEntity is object
                && loginEntity.Properties[nameof(EmailLoginEntity.State)].StringValue == nameof(EmailLoginEntityState.PendingConfirmation)
                && tokenEntity is object
                && tokenEntity.Properties[nameof(EmailLoginTokenEntity.Token)].StringValue == command.ConfirmationToken
                && (DateTime.UtcNow - tokenEntity.Properties[nameof(EmailLoginTokenEntity.Created)].DateTime.Value).TotalDays < 1)
            {
                loginEntity.Properties[nameof(EmailLoginEntity.State)].StringValue = nameof(EmailLoginEntityState.Confirmed);
                await _entityTables.Logins.ExecuteBatchAsync(
                    new TableBatchOperation
                    {
                        TableOperation.Merge(loginEntity),
                        TableOperation.Delete(tokenEntity)
                    },
                    cancellationToken
                );
            }
            else
                throw new PreconditionFailedException();
        }
    }
}
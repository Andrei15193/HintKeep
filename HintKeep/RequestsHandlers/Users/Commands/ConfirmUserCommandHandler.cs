using System;
using System.Collections.Generic;
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
    public class ConfirmUserCommandHandler : AsyncRequestHandler<ConfirmUserCommand>
    {
        private readonly IEntityTables _entityTables;

        public ConfirmUserCommandHandler(IEntityTables entityTables)
            => _entityTables = entityTables;

        protected override async Task Handle(ConfirmUserCommand command, CancellationToken cancellationToken)
        {
            var entities = await _entityTables.Users.ExecuteQuerySegmentedAsync(
                new TableQuery<UserActivationTokenEntity>()
                    .Where(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "UserActivationTokenEntity"),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition(nameof(TableEntity.RowKey), QueryComparisons.Equal, command.Token.ToEncodedKeyProperty())
                        )
                    )
                    .Take(1),
                null,
                cancellationToken
            );
            var userActivationTokenEntity = entities.SingleOrDefault();
            if (userActivationTokenEntity is null)
                throw new NotFoundException();
            var userEntity = (UserEntity)(await _entityTables.Users.ExecuteAsync(TableOperation.Retrieve<UserEntity>(userActivationTokenEntity.PartitionKey, "details", new List<string>()), cancellationToken)).Result;
            if (userEntity is null)
            {
                await _entityTables.Users.ExecuteAsync(TableOperation.Delete(userActivationTokenEntity), cancellationToken);
                throw new NotFoundException();
            }

            if (userActivationTokenEntity.Expiration < DateTimeOffset.UtcNow)
            {
                await _entityTables.Users.ExecuteBatchAsync(
                    new TableBatchOperation
                    {
                        TableOperation.Delete(userEntity),
                        TableOperation.Delete(userActivationTokenEntity)
                    },
                    cancellationToken
                );
                throw new NotFoundException();
            }
            else
            {
                await _entityTables.Users.ExecuteBatchAsync(
                    new TableBatchOperation
                    {
                        TableOperation.Merge(new DynamicTableEntity
                        {
                            PartitionKey = userEntity.PartitionKey,
                            RowKey = userEntity.RowKey,
                            ETag = userEntity.ETag,
                            Properties = new Dictionary<string, EntityProperty>(StringComparer.Ordinal)
                            {
                                { nameof(UserEntity.IsActive), EntityProperty.GeneratePropertyForBool(true) }
                            }
                        }),
                        TableOperation.Delete(userActivationTokenEntity)
                    },
                    cancellationToken
                );
            }
        }
    }
}
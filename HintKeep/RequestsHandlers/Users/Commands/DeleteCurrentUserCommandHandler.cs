using System.Collections.Generic;
using System.Net;
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
    public class DeleteCurrentUserCommandHandler : AsyncRequestHandler<DeleteCurrentUserCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _session;

        public DeleteCurrentUserCommandHandler(IEntityTables entityTables, Session session)
            => (_entityTables, _session) = (entityTables, session);

        protected override async Task Handle(DeleteCurrentUserCommand command, CancellationToken cancellationToken)
        {
            try
            {
                await _entityTables.Users.ExecuteAsync(TableOperation.Merge(new DynamicTableEntity
                {
                    PartitionKey = _session.UserId.ToEncodedKeyProperty(),
                    RowKey = "details".ToEncodedKeyProperty(),
                    ETag = "*",
                    Properties =
                    {
                        { nameof(UserEntity.IsDeleted), EntityProperty.GeneratePropertyForBool(true) }
                    }
                }));
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new UnauthorizedException(storageException);
            }
        }
    }
}
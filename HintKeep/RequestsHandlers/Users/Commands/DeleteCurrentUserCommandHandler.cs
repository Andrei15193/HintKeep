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
                var userEntity = (UserEntity)(await _entityTables.Users.ExecuteAsync(
                    TableOperation.Retrieve<UserEntity>(
                        _session.UserId.ToEncodedKeyProperty(),
                        "details".ToEncodedKeyProperty(),
                        new List<string>
                        {
                            nameof(UserEntity.Email)
                        }
                    ),
                    cancellationToken
                )).Result;

                await _entityTables.Users.ExecuteAsync(TableOperation.Delete(new TableEntity
                {
                    PartitionKey = _session.UserId.ToEncodedKeyProperty(),
                    RowKey = "details".ToEncodedKeyProperty(),
                    ETag = "*"
                }));

                try
                {
                    await _entityTables.Users.ExecuteAsync(TableOperation.Delete(new TableEntity
                    {
                        PartitionKey = userEntity.Email.ToLowerInvariant().ToEncodedKeyProperty(),
                        RowKey = nameof(LoginEntityType.EmailLogin).ToEncodedKeyProperty(),
                        ETag = "*"
                    }));
                }
                catch
                {
                }
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new UnauthorizedException(storageException);
            }
        }
    }
}
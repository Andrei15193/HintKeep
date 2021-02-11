using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Users.Commands;
using HintKeep.Storage;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.Users.Commands
{
    public class DeleteCurrentUserSessionCommandHandler : AsyncRequestHandler<DeleteCurrentUserSessionCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _session;

        public DeleteCurrentUserSessionCommandHandler(IEntityTables entityTables, Session session)
            => (_entityTables, _session) = (entityTables, session);

        protected override async Task Handle(DeleteCurrentUserSessionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _entityTables.UserSessions.ExecuteAsync(TableOperation.Delete(new TableEntity
                {
                    PartitionKey = _session.UserId.ToEncodedKeyProperty(),
                    RowKey = _session.SessionId.ToEncodedKeyProperty(),
                    ETag = "*"
                }));
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new UnauthorizedException(storageException);
            }
        }
    }
}
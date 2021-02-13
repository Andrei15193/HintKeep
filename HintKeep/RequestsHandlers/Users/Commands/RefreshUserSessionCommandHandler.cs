using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Users.Commands;
using HintKeep.Services;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.Users.Commands
{
    public class RefreshUserSessionCommandHandler : IRequestHandler<RefreshUserSessionCommand, UserSession>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _session;
        private readonly IJsonWebTokenService _jsonWebTokenService;

        public RefreshUserSessionCommandHandler(IEntityTables entityTables, Session session, IJsonWebTokenService jsonWebTokenService)
            => (_entityTables, _session, _jsonWebTokenService) = (entityTables, session, jsonWebTokenService);

        public async Task<UserSession> Handle(RefreshUserSessionCommand command, CancellationToken cancellationToken)
        {
            try
            {
                await _entityTables.UserSessions.ExecuteAsync(TableOperation.Merge(new DynamicTableEntity
                {
                    PartitionKey = _session.UserId.ToEncodedKeyProperty(),
                    RowKey = _session.SessionId.ToEncodedKeyProperty(),
                    ETag = "*",
                    Properties =
                    {
                        { nameof(UserSessionEntity.Expiration), EntityProperty.GeneratePropertyForDateTimeOffset(DateTime.UtcNow.AddHours(1)) }
                    }
                }));
                return new UserSession
                {
                    JsonWebToken = _jsonWebTokenService.GetJsonWebToken(_session.UserId, _session.SessionId)
                };
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new UnauthorizedException(storageException);
            }
        }
    }
}
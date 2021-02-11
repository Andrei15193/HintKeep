using System;
using System.Collections.Generic;
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
    public class CreateUserSessionCommandHandler : IRequestHandler<CreateUserSessionCommand, UserSession>
    {
        private readonly IEntityTables _entityTables;
        private readonly ICryptographicHashService _cryptographicHashService;
        private readonly IJsonWebTokenService _jsonWebTokenService;

        public CreateUserSessionCommandHandler(IEntityTables entityTables, ICryptographicHashService cryptographicHashService, IJsonWebTokenService jsonWebTokenService)
            => (_entityTables, _cryptographicHashService, _jsonWebTokenService) = (entityTables, cryptographicHashService, jsonWebTokenService);

        public async Task<UserSession> Handle(CreateUserSessionCommand command, CancellationToken cancellationToken)
        {
            var loginResult = await _entityTables.Logins.ExecuteAsync(
                TableOperation.Retrieve<EmailLoginEntity>(
                    command.Email.ToLowerInvariant().ToEncodedKeyProperty(),
                    nameof(LoginEntityType.EmailLogin).ToEncodedKeyProperty(),
                    new List<string>
                    {
                        nameof(EmailLoginEntity.PasswordSalt),
                        nameof(EmailLoginEntity.PasswordHash),
                        nameof(EmailLoginEntity.State),
                        nameof(EmailLoginEntity.UserId)
                    }
                ),
                cancellationToken
            );
            if (!(loginResult.Result is EmailLoginEntity loginEntity)
                || loginEntity.PasswordHash != _cryptographicHashService.GetHash(loginEntity.PasswordSalt + command.Password)
                || loginEntity.State != nameof(EmailLoginEntityState.Confirmed))
                throw new UnauthorizedException();

            var sessionId = Guid.NewGuid().ToString("N");
            await _entityTables.UserSessions.ExecuteAsync(
                TableOperation.Insert(new UserSessionEntity
                {
                    EntityType = "UserSessionEntity",
                    PartitionKey = loginEntity.UserId.ToEncodedKeyProperty(),
                    RowKey = sessionId.ToEncodedKeyProperty(),
                    Expiration = DateTime.UtcNow.AddHours(1)
                }),
                cancellationToken
            );

            return new UserSession
            {
                JsonWebToken = _jsonWebTokenService.GetJsonWebToken(loginEntity.UserId, sessionId)
            };
        }
    }
}
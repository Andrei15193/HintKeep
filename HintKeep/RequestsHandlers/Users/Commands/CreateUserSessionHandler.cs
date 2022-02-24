using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Services;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.Users.Commands
{
    public class CreateUserSessionCommandHandler : IRequestHandler<CreateUserSessionCommand, string>
    {
        private readonly IEntityTables _entityTables;
        private readonly ISecurityService _securityService;
        private readonly ISessionService _sessionService;

        public CreateUserSessionCommandHandler(IEntityTables entityTables, ISecurityService securityService, ISessionService sessionService)
            => (_entityTables, _securityService, _sessionService) = (entityTables, securityService, sessionService);

        public async Task<string> Handle(CreateUserSessionCommand command, CancellationToken cancellationToken)
        {
            var emailHash = _securityService.ComputeHash(command.Email.ToLowerInvariant());
            var userEntity = (UserEntity)(await _entityTables.Users.ExecuteAsync(
                TableOperation.Retrieve<UserEntity>(
                    emailHash.ToEncodedKeyProperty(),
                    "details".ToEncodedKeyProperty(),
                    new List<string>
                    {
                        nameof(UserEntity.IsActive),
                        nameof(UserEntity.PasswordSalt),
                        nameof(UserEntity.PasswordHash),
                        nameof(UserEntity.Id),
                        nameof(UserEntity.Role)
                    }),
                cancellationToken
            )).Result;
            if (userEntity is null || !userEntity.IsActive)
                throw new NotFoundException();

            if (userEntity.PasswordHash != _securityService.ComputePasswordHash(userEntity.PasswordSalt, command.Password))
                throw new ValidationException("errors.login.invalidCredentials");

            var jsonWebToken = _sessionService.CreateJsonWebToken(userEntity.Id, userEntity.Role);
            await _entityTables.Users.ExecuteAsync(
                TableOperation.Merge(new DynamicTableEntity
                {
                    PartitionKey = userEntity.PartitionKey,
                    RowKey = userEntity.RowKey,
                    ETag = userEntity.ETag,
                    Properties =
                    {
                        { nameof(UserEntity.LastLoginTime), EntityProperty.GeneratePropertyForDateTimeOffset(DateTimeOffset.UtcNow) }
                    }
                }),
                cancellationToken
            );

            return jsonWebToken;
        }
    }
}
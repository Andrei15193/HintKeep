using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Users.Queries;
using HintKeep.Services;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.Users.Queries
{
    public class UserAuthenticationQueryHandler : IRequestHandler<UserAuthenticationQuery, UserInfo>
    {
        private readonly IEntityTables _entityTables;
        private readonly ICryptographicHashService _cryptographicHashService;
        private readonly IJsonWebTokenService _jsonWebTokenService;

        public UserAuthenticationQueryHandler(IEntityTables entityTables, ICryptographicHashService cryptographicHashService, IJsonWebTokenService jsonWebTokenService)
            => (_entityTables, _cryptographicHashService, _jsonWebTokenService) = (entityTables, cryptographicHashService, jsonWebTokenService);

        public async Task<UserInfo> Handle(UserAuthenticationQuery query, CancellationToken cancellationToken)
        {
            var userResult = await _entityTables.Users.ExecuteAsync(
                TableOperation.Retrieve<UserEntity>(
                    query.Email.ToLowerInvariant(),
                    "user",
                    new List<string> { nameof(UserEntity.Email), nameof(UserEntity.PasswordSalt), nameof(UserEntity.PasswordHash), nameof(UserEntity.State) }
                ),
                cancellationToken
            );
            if (!(userResult.Result is UserEntity userEntity)
                || userEntity.PasswordHash != _cryptographicHashService.GetHash(userEntity.PasswordSalt + query.Password)
                || userEntity.State != (int)UserState.Confirmed)
                throw new UnauthorizedException();

            return new UserInfo
            {
                JsonWebToken = _jsonWebTokenService.GetJsonWebToken(userEntity.Email)
            };
        }
    }
}
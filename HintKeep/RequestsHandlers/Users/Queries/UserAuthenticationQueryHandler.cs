using System.Collections.Generic;
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
            var loginResult = await _entityTables.Logins.ExecuteAsync(
                TableOperation.Retrieve<EmailLoginEntity>(
                    query.Email.ToLowerInvariant().ToEncodedKeyProperty(),
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
                || loginEntity.PasswordHash != _cryptographicHashService.GetHash(loginEntity.PasswordSalt + query.Password)
                || loginEntity.State != nameof(EmailLoginEntityState.Confirmed))
                throw new UnauthorizedException();

            return new UserInfo
            {
                JsonWebToken = _jsonWebTokenService.GetJsonWebToken(loginEntity.UserId)
            };
        }
    }
}
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace HintKeep.Services.Implementations
{
    public class SessionService : ISessionService
    {
        private readonly SessionServiceConfig _config;

        public SessionService(SessionServiceConfig config)
            => _config = config;

        public string CreateJsonWebToken(string userId, string userRole)
        {
            var token = new JwtSecurityToken
            (
                _config.ApplicationId,
                _config.Audience,
                new[]
                {
                    new Claim(ClaimTypes.Name, userId),
                    new Claim(ClaimTypes.Role, userRole),
                    new Claim(JwtRegisteredClaimNames.Sub, _config.ApplicationId),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                },
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddHours(1),
                new SigningCredentials(_config.SigningKey, _config.SingingAlgorithm)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
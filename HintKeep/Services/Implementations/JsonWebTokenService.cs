using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HintKeep.Services.Implementations.Configs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HintKeep.Services.Implementations
{
    public class JsonWebTokenService : IJsonWebTokenService
    {
        private readonly JsonWebTokenServiceConfig _config;

        public JsonWebTokenService(JsonWebTokenServiceConfig config)
            => _config = config;

        public string GetJsonWebToken(string userEmail)
            => _GetJsonWebToken(new Claim(ClaimTypes.Name, userEmail));

        private string _GetJsonWebToken(params Claim[] claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    claims,
                    JwtBearerDefaults.AuthenticationScheme
                ),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = _config.SigningCredentials
            };
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var jsonWebToken = tokenHandler.WriteToken(securityToken);
            return jsonWebToken;
        }
    }
}
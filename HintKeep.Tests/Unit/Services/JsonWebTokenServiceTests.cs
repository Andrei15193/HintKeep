using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using HintKeep.Services.Implementations;
using HintKeep.Services.Implementations.Configs;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace HintKeep.Tests.Unit.Services
{
    public class JsonWebTokenServiceTests
    {
        [Fact]
        public void GetJsonWebToken_GetsJwt_IsNotEncrypted()
        {
            var jsonWebTokenService = new JsonWebTokenService(ServiceConfigFactory.Create<JsonWebTokenServiceConfig>(new { SigningKey = "test-signing-key", SigningAlgorithm = "HmacSha256Signature" }));

            var jsonWebToken = jsonWebTokenService.GetJsonWebToken("user-id", "session-id");

            var securityToken = new JwtSecurityTokenHandler().ReadJwtToken(jsonWebToken);
            Assert.Equal("user-id", securityToken.Claims.Single(claim => claim.Type == "unique_name").Value);
            Assert.Equal("session-id", securityToken.Claims.Single(claim => claim.Type == "certserialnumber").Value);
        }

        [Fact]
        public void GetJsonWebToken_GetsJwt_ThatCanBeRead()
        {
            var jsonWebTokenService = new JsonWebTokenService(ServiceConfigFactory.Create<JsonWebTokenServiceConfig>(new { SigningKey = "test-signing-key", SigningAlgorithm = "HmacSha256Signature" }));

            var jsonWebToken = jsonWebTokenService.GetJsonWebToken("user-id", "session-id");

            var claims = new JwtSecurityTokenHandler().ValidateToken(
                jsonWebToken,
                new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-signing-key"))
                },
                out var securityToken);
            Assert.Equal("user-id", claims.FindAll(claim => claim.Type == ClaimTypes.Name).Single().Value);
            Assert.Equal("session-id", claims.FindAll(claim => claim.Type == ClaimTypes.SerialNumber).Single().Value);
        }

        [Fact]
        public void GetJsonWebToken_GetsJwt_CannotValidateWithDifferentSigningKey()
        {
            var jsonWebTokenService = new JsonWebTokenService(ServiceConfigFactory.Create<JsonWebTokenServiceConfig>(new { SigningKey = "test-signing-key", SigningAlgorithm = "HmacSha256Signature" }));

            var jsonWebToken = jsonWebTokenService.GetJsonWebToken("user-id", "session-id");

            Assert.Throws<SecurityTokenInvalidSignatureException>(
                () => new JwtSecurityTokenHandler().ValidateToken(
                    jsonWebToken,
                    new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-signing-key-bad"))
                    },
                    out var securityToken
                )
            );
        }
    }
}
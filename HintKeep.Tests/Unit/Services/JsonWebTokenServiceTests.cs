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

            var jsonWebToken = jsonWebTokenService.GetJsonWebToken("test-eMail");

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = new JwtSecurityTokenHandler().ReadJwtToken(jsonWebToken);
            Assert.Equal("test-eMail", securityToken.Claims.Single(claim => claim.Type == "unique_name").Value);
        }

        [Fact]
        public void GetJsonWebToken_GetsJwt_ThatCanBeRead()
        {
            var jsonWebTokenService = new JsonWebTokenService(ServiceConfigFactory.Create<JsonWebTokenServiceConfig>(new { SigningKey = "test-signing-key", SigningAlgorithm = "HmacSha256Signature" }));

            var jsonWebToken = jsonWebTokenService.GetJsonWebToken("test-eMail");

            var tokenHandler = new JwtSecurityTokenHandler();
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
            Assert.Equal("test-eMail", claims.FindAll(claim => claim.Type == ClaimTypes.Name).Single().Value);
        }

        [Fact]
        public void GetJsonWebToken_GetsJwt_CannotValidateWithDifferentSigningKey()
        {
            var jsonWebTokenService = new JsonWebTokenService(ServiceConfigFactory.Create<JsonWebTokenServiceConfig>(new { SigningKey = "test-signing-key", SigningAlgorithm = "HmacSha256Signature" }));

            var jsonWebToken = jsonWebTokenService.GetJsonWebToken("test-eMail");

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
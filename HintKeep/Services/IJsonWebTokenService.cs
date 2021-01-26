using System;

namespace HintKeep.Services
{
    public interface IJsonWebTokenService
    {
        string GetJsonWebToken(Guid userEmail);
    }
}
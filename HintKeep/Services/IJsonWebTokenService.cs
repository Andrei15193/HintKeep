namespace HintKeep.Services
{
    public interface IJsonWebTokenService
    {
        string GetJsonWebToken(string userEmail);
    }
}
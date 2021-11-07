namespace HintKeep.Services
{
    public interface ISessionService
    {
        string CreateJsonWebToken(string userId, string userRole);
    }
}
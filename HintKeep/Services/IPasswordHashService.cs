namespace HintKeep.Services
{
    public interface IPasswordHashService
    {
        string GetHash(string salt, string password);
    }
}
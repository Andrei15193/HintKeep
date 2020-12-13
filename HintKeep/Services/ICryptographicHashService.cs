namespace HintKeep.Services
{
    public interface ICryptographicHashService
    {
        string GetHash(string value);
    }
}
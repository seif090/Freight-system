namespace FreightSystem.Application.Interfaces
{
    public interface IApiKeyManager
    {
        bool ValidateKey(string apiKey);
        string CreateKey(string owner);
        IEnumerable<string> ListKeys();
    }
}

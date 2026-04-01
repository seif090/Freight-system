using FreightSystem.Application.Interfaces;

namespace FreightSystem.Infrastructure.Services
{
    public class ApiKeyManager : IApiKeyManager
    {
        private static readonly Dictionary<string, string> _apiKeys = new();

        public bool ValidateKey(string apiKey)
        {
            return !string.IsNullOrWhiteSpace(apiKey) && _apiKeys.ContainsKey(apiKey);
        }

        public string CreateKey(string owner)
        {
            var key = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", "").Replace("+", "").Replace("/", "");
            _apiKeys[key] = owner ?? "unknown";
            return key;
        }

        public IEnumerable<string> ListKeys() => _apiKeys.Keys;
    }
}

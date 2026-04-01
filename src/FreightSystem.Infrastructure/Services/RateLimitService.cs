using FreightSystem.Application.Interfaces;

namespace FreightSystem.Infrastructure.Services
{
    public class RateLimitService : IRateLimitService
    {
        private static readonly Dictionary<string, (int Count, DateTime WindowStart)> _requests = new();
        private readonly int _limit = 60;
        private readonly TimeSpan _window = TimeSpan.FromMinutes(1);

        public bool AllowRequest(string key)
        {
            lock (_requests)
            {
                if (!_requests.TryGetValue(key, out var data) || DateTime.UtcNow - data.WindowStart > _window)
                {
                    _requests[key] = (1, DateTime.UtcNow);
                    return true;
                }

                if (data.Count >= _limit)
                    return false;

                _requests[key] = (data.Count + 1, data.WindowStart);
                return true;
            }
        }
    }
}

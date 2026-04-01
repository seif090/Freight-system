namespace FreightSystem.Application.Interfaces
{
    public interface IRateLimitService
    {
        bool AllowRequest(string key);
    }
}

using FreightSystem.Core.Entities;

namespace FreightSystem.Application.Interfaces
{
    public interface ITelematicsService
    {
        Task AddDataAsync(TelematicsData data);
        Task<IEnumerable<TelematicsData>> GetByShipmentAsync(int shipmentId);
    }

    public interface ITrafficService
    {
        Task<string> GetTrafficForecastAsync(double latitude, double longitude);
    }

    public interface IAiTrainingService
    {
        Task<string> QueueTrainingAsync();
        Task<string> GetTrainingStatusAsync();
    }
}
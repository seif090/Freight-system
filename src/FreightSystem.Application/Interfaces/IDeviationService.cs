using FreightSystem.Core.Entities;

namespace FreightSystem.Application.Interfaces
{
    public interface IDeviationService
    {
        Task<RouteDeviationAlert> EvaluateRouteDeviationAsync(int shipmentId, double currentLat, double currentLon, IEnumerable<Core.Entities.RouteSegment> plannedSegments);
        Task<IEnumerable<RouteDeviationAlert>> GetRecentAlertsAsync(int page, int pageSize);
        Task<RouteDeviationAlert> ResolveAlertAsync(int alertId);
    }
}
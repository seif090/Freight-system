using FreightSystem.Core.Entities;

namespace FreightSystem.Application.Interfaces
{
    public class RouteOptimizationResult
    {
        public int ShipmentId { get; set; }
        public IEnumerable<RouteSegment> OptimizedTrajectory { get; set; } = Array.Empty<RouteSegment>();
        public string Reason { get; set; } = string.Empty;
    }

    public interface IRouteOptimizationService
    {
        Task<RouteOptimizationResult> OptimizeRouteAsync(int shipmentId, IEnumerable<RouteSegment> plannedSegments);
        Task<IEnumerable<Vehicle>> EvaluateMaintenanceRiskAsync(string tenantId);
    }
}

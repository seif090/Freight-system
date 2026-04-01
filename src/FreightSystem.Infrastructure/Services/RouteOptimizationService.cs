using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FreightSystem.Infrastructure.Services
{
    public class RouteOptimizationService : IRouteOptimizationService
    {
        private readonly FreightDbContext _dbContext;

        public RouteOptimizationService(FreightDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RouteOptimizationResult> OptimizeRouteAsync(int shipmentId, IEnumerable<RouteSegment> plannedSegments)
        {
            var shipment = await _dbContext.Shipments.FindAsync(shipmentId);
            if (shipment is null)
                return new RouteOptimizationResult { ShipmentId = shipmentId, Reason = "Shipment not found" };

            // simple heuristic: reorder by segment distance ascending to reduce transfer delay.
            var optimized = plannedSegments.OrderBy(x => x.DistanceKm).ToList();
            var updates = optimized.Select((seg, idx) => { seg.SegmentOrder = idx + 1; return seg; }).ToList();

            return new RouteOptimizationResult
            {
                ShipmentId = shipmentId,
                OptimizedTrajectory = updates,
                Reason = "Optimized by distance and ETA stabilizer"
            };
        }

        public async Task<IEnumerable<Vehicle>> EvaluateMaintenanceRiskAsync(string tenantId)
        {
            var now = DateTime.UtcNow;
            var candidates = await _dbContext.Vehicles
                .Include(v => v.MaintenanceEvents)
                .Where(v => v.TenantId == tenantId && v.Status == VehicleStatus.Active)
                .ToListAsync();

            return candidates.Where(v => v.NextInspectionDue <= now.AddDays(14) || v.MaintenanceEvents.Any(ev => ev.Cost > 1000));
        }
    }
}

using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FreightSystem.Infrastructure.Services
{
    public class DeviationService : IDeviationService
    {
        private readonly FreightDbContext _dbContext;

        public DeviationService(FreightDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RouteDeviationAlert> EvaluateRouteDeviationAsync(int shipmentId, double currentLat, double currentLon, IEnumerable<RouteSegment> plannedSegments)
        {
            // basic path deviation using closest waypoint
            var minDistance = double.MaxValue;
            var closest = plannedSegments.FirstOrDefault();

            foreach (var segment in plannedSegments)
            {
                var dist = GetDistanceInMeters(currentLat, currentLon, segment.StartLatitude, segment.StartLongitude);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = segment;
                }
                var dist2 = GetDistanceInMeters(currentLat, currentLon, segment.EndLatitude, segment.EndLongitude);
                if (dist2 < minDistance)
                {
                    minDistance = dist2;
                    closest = segment;
                }
            }

            var alert = new RouteDeviationAlert
            {
                ShipmentId = shipmentId,
                DeviationMeters = minDistance,
                CurrentLatitude = currentLat,
                CurrentLongitude = currentLon,
                Status = minDistance > 500 ? "Deviation" : "Within tolerance",
                Notes = minDistance > 500 ? "Significant deviation from planned route." : "Minor discrepancy.",
                CreatedAt = DateTime.UtcNow,
                Actioned = false
            };

            _dbContext.RouteDeviationAlerts.Add(alert);
            await _dbContext.SaveChangesAsync();

            return alert;
        }

        public async Task<IEnumerable<RouteDeviationAlert>> GetRecentAlertsAsync(int page, int pageSize)
        {
            return await _dbContext.RouteDeviationAlerts
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<RouteDeviationAlert> ResolveAlertAsync(int alertId)
        {
            var alert = await _dbContext.RouteDeviationAlerts.FindAsync(alertId);
            if (alert == null) throw new KeyNotFoundException();
            alert.Actioned = true;
            alert.Status = "Resolved";
            await _dbContext.SaveChangesAsync();
            return alert;
        }

        private double GetDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            static double ToRad(double d) => d * Math.PI / 180;
            var R = 6371000.0;
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
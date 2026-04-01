using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FreightSystem.Infrastructure.Services
{
    public class GeoService : IGeoService
    {
        private readonly FreightDbContext _dbContext;

        public GeoService(FreightDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GeofenceCheckResult> CheckGeofenceAsync(double latitude, double longitude, string tenantId)
        {
            var activeGeofences = await _dbContext.Geofences
                .Where(x => x.IsActive && x.TenantId == tenantId)
                .ToListAsync();

            foreach (var geofence in activeGeofences)
            {
                var distance = GetDistanceInMeters(latitude, longitude, geofence.CenterLatitude, geofence.CenterLongitude);
                if (distance <= geofence.RadiusMeters)
                {
                    return new GeofenceCheckResult
                    {
                        InsideAnyGeofence = true,
                        GeofenceName = geofence.Name,
                        DistanceToBoundaryMeters = geofence.RadiusMeters - distance
                    };
                }
            }

            return new GeofenceCheckResult { InsideAnyGeofence = false, DistanceToBoundaryMeters = double.MaxValue };
        }

        public Task<SegmentEtaDetail> CalculateSegmentEtaAsync(RouteSegment segment, Shipment shipment)
        {
            var now = DateTime.UtcNow;
            var speedKmh = 48.0; // sample speed or use dynamic routing data from ML
            var travelTimeMinutes = segment.DistanceKm / speedKmh * 60;
            var predictedArrival = now.AddMinutes(travelTimeMinutes);
            var delay = (shipment.ETA.HasValue ? (predictedArrival - shipment.ETA.Value).TotalMinutes : 0);

            return Task.FromResult(new SegmentEtaDetail
            {
                SegmentOrder = segment.SegmentOrder,
                DistanceKm = segment.DistanceKm,
                EstimatedArrival = predictedArrival,
                EstimatedDelayMinutes = Math.Max(0, delay)
            });
        }

        public double GetDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var rLat1 = ToRadians(lat1);
            var rLat2 = ToRadians(lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(rLat1) * Math.Cos(rLat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var earthRadiusMeters = 6371000;
            return earthRadiusMeters * c;
        }
    }
}
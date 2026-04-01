using FreightSystem.Core.Entities;

namespace FreightSystem.Application.Interfaces
{
    public class GeofenceCheckResult
    {
        public bool InsideAnyGeofence { get; set; }
        public string? GeofenceName { get; set; }
        public double DistanceToBoundaryMeters { get; set; }
    }

    public class SegmentEtaDetail
    {
        public int SegmentOrder { get; set; }
        public double DistanceKm { get; set; }
        public DateTime EstimatedArrival { get; set; }
        public double EstimatedDelayMinutes { get; set; }
    }

    public interface IGeoService
    {
        Task<GeofenceCheckResult> CheckGeofenceAsync(double latitude, double longitude, string tenantId);
        Task<SegmentEtaDetail> CalculateSegmentEtaAsync(RouteSegment segment, Shipment shipment);
        double GetDistanceInMeters(double lat1, double lon1, double lat2, double lon2);
    }
}
namespace FreightSystem.Core.Entities
{
    public class RouteSegment
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }

        public int SegmentOrder { get; set; }

        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public double EndLatitude { get; set; }
        public double EndLongitude { get; set; }

        public double DistanceKm { get; set; }
        public double DurationMinutes { get; set; }
        public DateTime? EstimatedArrival { get; set; }
        public double PredictedDelayMinutes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string TenantId { get; set; } = "default";
    }
}
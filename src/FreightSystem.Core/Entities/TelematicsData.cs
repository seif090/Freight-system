namespace FreightSystem.Core.Entities
{
    public class TelematicsData
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double SpeedKmh { get; set; }
        public double HeadingDegrees { get; set; }
        public double FuelLevel { get; set; }
        public string Provider { get; set; } = "unknown";
        public string TenantId { get; set; } = "default";
    }
}
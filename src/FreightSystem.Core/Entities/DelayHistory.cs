namespace FreightSystem.Core.Entities
{
    public class DelayHistory
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }
        public DateTime? ETD { get; set; }
        public DateTime? ETA { get; set; }
        public DateTime ActualDeparture { get; set; }
        public DateTime ActualArrival { get; set; }
        public double DurationHours { get; set; }
        public double DelayMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TenantId { get; set; } = "default";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
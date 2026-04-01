namespace FreightSystem.Core.Entities
{
    public class WarehouseShipmentFact
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string RouteKey { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime? ETD { get; set; }
        public DateTime? ETA { get; set; }
        public DateTime FactDate { get; set; } = DateTime.UtcNow;
        public string TenantId { get; set; } = "default";
        public bool IsDelayAnomaly { get; set; }
    }
}
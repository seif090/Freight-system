namespace FreightSystem.Core.Entities
{
    public class ShipmentLocationHistory
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public ShipmentStatus? Status { get; set; }
    }
}

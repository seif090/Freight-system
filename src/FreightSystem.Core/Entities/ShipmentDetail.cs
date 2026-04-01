namespace FreightSystem.Core.Entities
{
    public class ShipmentDetail
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;

        public string Commodity { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public int Quantity { get; set; }

        public string Remarks { get; set; } = string.Empty;
    }
}

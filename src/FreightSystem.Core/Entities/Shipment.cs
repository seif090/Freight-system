namespace FreightSystem.Core.Entities
{
    public enum ShipmentType { Import, Export, Domestic }
    public enum TransportMode { Sea, Air, Land }
    public enum ShipmentStatus { Pending, InTransit, Delivered, Cancelled }
    public enum ContainerType { None, TwentyFt, FortyFt, LCL, FCL }
    public enum ShipmentPriority { Low, Normal, High, Critical }
    public enum RouteRiskLevel { Low, Medium, High, Critical }

    public class Shipment
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public ShipmentType Type { get; set; }
        public TransportMode Mode { get; set; }
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
        public ShipmentPriority Priority { get; set; } = ShipmentPriority.Normal;

        public string PortOfLoading { get; set; } = string.Empty;
        public string PortOfDischarge { get; set; } = string.Empty;
        public DateTime? ETD { get; set; }
        public DateTime? ETA { get; set; }
        public ContainerType ContainerType { get; set; }
        public string VesselOrFlightNumber { get; set; } = string.Empty;
        public double? OriginLatitude { get; set; }
        public double? OriginLongitude { get; set; }
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public string TenantId { get; set; } = "default";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<ShipmentDetail> Details { get; set; } = new List<ShipmentDetail>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}

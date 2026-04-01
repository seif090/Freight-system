namespace FreightSystem.Core.Entities
{
    public class RouteDeviationAlert
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double DeviationMeters { get; set; }
        public double CurrentLatitude { get; set; }
        public double CurrentLongitude { get; set; }
        public string Status { get; set; } = "New";
        public string Notes { get; set; } = string.Empty;
        public bool Actioned { get; set; } = false;
    }
}
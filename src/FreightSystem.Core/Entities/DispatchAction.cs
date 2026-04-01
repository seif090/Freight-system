namespace FreightSystem.Core.Entities
{
    public class DispatchAction
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public string Instruction { get; set; } = string.Empty;
        public string RoutePreviewUrl { get; set; } = string.Empty;
        public string RouteGeoJson { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
        public bool Dispatched { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DispatchedAt { get; set; }
        public string TenantId { get; set; } = "default";
    }
}
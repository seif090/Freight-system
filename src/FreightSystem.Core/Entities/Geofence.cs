namespace FreightSystem.Core.Entities
{
    public class Geofence
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double CenterLatitude { get; set; }
        public double CenterLongitude { get; set; }
        public double RadiusMeters { get; set; }
        public bool IsActive { get; set; } = true;
        public string TenantId { get; set; } = "default";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
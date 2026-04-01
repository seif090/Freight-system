namespace FreightSystem.Core.Entities
{
    public enum VehicleStatus { Active, InMaintenance, Retired }

    public class Vehicle
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public DateTime LastInspection { get; set; } = DateTime.UtcNow;
        public DateTime NextInspectionDue { get; set; } = DateTime.UtcNow.AddMonths(3);
        public VehicleStatus Status { get; set; } = VehicleStatus.Active;
        public string TenantId { get; set; } = "default";
        public ICollection<MaintenanceEvent> MaintenanceEvents { get; set; } = new List<MaintenanceEvent>();
    }
}
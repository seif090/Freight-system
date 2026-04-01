namespace FreightSystem.Core.Entities
{
    public enum MaintenanceType { Inspection, Repair, PartsReplacement, OilChange }

    public class MaintenanceEvent
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }
        public MaintenanceType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public double Cost { get; set; }
    }
}
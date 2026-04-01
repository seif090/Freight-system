namespace FreightSystem.Core.Entities
{
    public class BlockchainAudit
    {
        public int Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string PayloadHash { get; set; } = string.Empty;
        public string PreviousHash { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
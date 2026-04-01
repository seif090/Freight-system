namespace FreightSystem.Core.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }
    }
}

namespace FreightSystem.Core.Entities
{
    public class LlmSpendLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; } = "unknown";
        public string Input { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public int TokenUsage { get; set; }
        public double EstimatedCostUsd { get; set; }
        public bool Success { get; set; }
    }
}
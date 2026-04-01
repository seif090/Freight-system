namespace FreightSystem.Core.Entities
{
    public class DelayAnomalyClusterHistory
    {
        public int Id { get; set; }
        public DateTime WeekStarting { get; set; }
        public double ThresholdMinutes { get; set; }
        public int TotalClusters { get; set; }
        public int TotalMatches { get; set; }
        public double AvgDelayMinutes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
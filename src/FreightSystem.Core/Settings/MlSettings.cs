namespace FreightSystem.Core.Settings
{
    public class MlSettings
    {
        public string ApiBaseUrl { get; set; } = "http://localhost:8000";
        public string StreamEndpoint { get; set; } = "/api/ml/stream";
        public string AnomalyEndpoint { get; set; } = "/api/ml/anomaly";
        public string EtaEndpoint { get; set; } = "/api/ml/eta";
    }
}
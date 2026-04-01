using FreightSystem.Core.Entities;

namespace FreightSystem.Application.Interfaces
{
    public class DelayAnomalyResponse
    {
        public int ShipmentId { get; set; }
        public bool IsAnomaly { get; set; }
        public double DelayMinutes { get; set; }
        public string? Forecast { get; set; }
    }

    public class EtaPredictionResponse
    {
        public int ShipmentId { get; set; }
        public DateTime PredictedETA { get; set; }
        public double Confidence { get; set; }
        public double PredictedDelayMinutes { get; set; }
    }

    public class DelayRegressionResult
    {
        public double Slope { get; set; }
        public double Intercept { get; set; }
        public double RSquared { get; set; }
        public double ForecastDelayMinutes { get; set; }
        public string Model { get; set; } = "Linear";
        public int SampleSize { get; set; }
        public IEnumerable<object> Samples { get; set; } = Array.Empty<object>();
    }

    public interface IMLService
    {
        Task<DelayAnomalyResponse> AnalyzeDelayAsync(Shipment shipment);
        Task<EtaPredictionResponse> PredictETAAsync(Shipment shipment, IEnumerable<RouteSegment> segments);
        Task<bool> StreamRouteDataAsync(int shipmentId, IEnumerable<ShipmentLocationHistory> history);
        Task<DelayRegressionResult> PredictDelayRegressionAsync(IEnumerable<Shipment> shipments);
    }
}
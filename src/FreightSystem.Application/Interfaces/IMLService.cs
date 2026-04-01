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

    public interface IMLService
    {
        Task<DelayAnomalyResponse> AnalyzeDelayAsync(Shipment shipment);
        Task<EtaPredictionResponse> PredictETAAsync(Shipment shipment, IEnumerable<RouteSegment> segments);
        Task<bool> StreamRouteDataAsync(int shipmentId, IEnumerable<ShipmentLocationHistory> history);
    }
}
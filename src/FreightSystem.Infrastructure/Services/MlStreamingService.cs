using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Core.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace FreightSystem.Infrastructure.Services
{
    public class MlStreamingService : IMLService
    {
        private readonly HttpClient _httpClient;
        private readonly MlSettings _settings;

        public MlStreamingService(HttpClient httpClient, IOptions<MlSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            if (!string.IsNullOrWhiteSpace(_settings.ApiBaseUrl))
                _httpClient.BaseAddress = new Uri(_settings.ApiBaseUrl);
        }

        public async Task<bool> StreamRouteDataAsync(int shipmentId, IEnumerable<ShipmentLocationHistory> history)
        {
            var payload = new
            {
                ShipmentId = shipmentId,
                TrackPoints = history.Select(x => new { x.Timestamp, x.Latitude, x.Longitude, x.Status })
            };

            var response = await _httpClient.PostAsJsonAsync(_settings.StreamEndpoint, payload);
            return response.IsSuccessStatusCode;
        }

        public async Task<DelayAnomalyResponse> AnalyzeDelayAsync(Shipment shipment)
        {
            var request = new { shipment.Id, shipment.TrackingNumber, shipment.Status, shipment.ETD, shipment.ETA, shipment.CurrentLatitude, shipment.CurrentLongitude };
            var response = await _httpClient.PostAsJsonAsync(_settings.AnomalyEndpoint, request);

            if (!response.IsSuccessStatusCode)
            {
                return new DelayAnomalyResponse { ShipmentId = shipment.Id, IsAnomaly = false, DelayMinutes = 0, Forecast = "Unavailable" };
            }

            var data = await response.Content.ReadFromJsonAsync<DelayAnomalyResponse>();
            if (data is null)
            {
                return new DelayAnomalyResponse { ShipmentId = shipment.Id, IsAnomaly = false, DelayMinutes = 0, Forecast = "Invalid response" };
            }

            return data;
        }

        public async Task<EtaPredictionResponse> PredictETAAsync(Shipment shipment, IEnumerable<RouteSegment> segments)
        {
            var request = new
            {
                shipment.Id,
                shipment.TrackingNumber,
                shipment.Status,
                shipment.ETD,
                shipment.ETA,
                RouteSegments = segments.Select(s => new { s.SegmentOrder, s.StartLatitude, s.StartLongitude, s.EndLatitude, s.EndLongitude, s.DistanceKm })
            };

            var response = await _httpClient.PostAsJsonAsync(_settings.EtaEndpoint, request);
            if (!response.IsSuccessStatusCode)
            {
                return new EtaPredictionResponse { ShipmentId = shipment.Id, PredictedETA = shipment.ETA ?? DateTime.UtcNow.AddHours(1), Confidence = 0.0, PredictedDelayMinutes = 0.0 };
            }

            var data = await response.Content.ReadFromJsonAsync<EtaPredictionResponse>();
            if (data is null)
            {
                return new EtaPredictionResponse { ShipmentId = shipment.Id, PredictedETA = shipment.ETA ?? DateTime.UtcNow.AddHours(1), Confidence = 0.0, PredictedDelayMinutes = 0.0 };
            }

            return data;
        }

        public Task<DelayRegressionResult> PredictDelayRegressionAsync(IEnumerable<Shipment> shipments)
        {
            var sample = shipments
                .Where(s => s.ETD.HasValue && s.ETA.HasValue && s.Status == ShipmentStatus.Delivered && s.UpdatedAt.HasValue)
                .Select(s => new
                {
                    DurationHours = (s.ETA.Value - s.ETD.Value).TotalHours,
                    DelayMinutes = (s.UpdatedAt.Value - s.ETA.Value).TotalMinutes
                })
                .Where(x => !double.IsNaN(x.DurationHours) && !double.IsInfinity(x.DurationHours))
                .ToList();

            if (sample.Count < 2)
            {
                return Task.FromResult(new DelayRegressionResult
                {
                    Slope = 0,
                    Intercept = 0,
                    RSquared = 0,
                    ForecastDelayMinutes = 0,
                    SampleSize = sample.Count,
                    Samples = sample.Select(x => new { Hours = x.DurationHours, Delay = x.DelayMinutes })
                });
            }

            var xVals = sample.Select(x => x.DurationHours).ToArray();
            var yVals = sample.Select(x => x.DelayMinutes).ToArray();

            var xMean = xVals.Average();
            var yMean = yVals.Average();
            var ssX = xVals.Sum(x => Math.Pow(x - xMean, 2));
            var ssXY = xVals.Zip(yVals, (x, y) => (x - xMean) * (y - yMean)).Sum();
            var slope = ssX != 0 ? ssXY / ssX : 0;
            var intercept = yMean - slope * xMean;

            var ssTot = yVals.Sum(y => Math.Pow(y - yMean, 2));
            var ssRes = xVals.Zip(yVals, (x, y) => Math.Pow(y - (slope * x + intercept), 2)).Sum();
            var rSquared = ssTot == 0 ? 0 : 1 - ssRes / ssTot;

            // forecast for average duration
            var avgDuration = xMean;
            var forecast = slope * avgDuration + intercept;

            return Task.FromResult(new DelayRegressionResult
            {
                Slope = slope,
                Intercept = intercept,
                RSquared = rSquared,
                ForecastDelayMinutes = forecast,
                SampleSize = sample.Count,
                Samples = sample.Select(x => new { Hours = x.DurationHours, Delay = x.DelayMinutes })
            });
        }
    }
}
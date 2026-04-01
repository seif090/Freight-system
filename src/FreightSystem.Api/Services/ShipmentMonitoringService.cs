using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Api.Hubs;
using FreightSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FreightSystem.Api.Services
{
    public class ShipmentMonitoringService
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly INotificationService _notificationService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<LiveTrackingHub> _hubContext;
        private readonly FreightDbContext _dbContext;

        public ShipmentMonitoringService(
            IShipmentRepository shipmentRepository,
            INotificationService notificationService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHubContext<LiveTrackingHub> hubContext,
            FreightDbContext dbContext)
        {
            _shipmentRepository = shipmentRepository;
            _notificationService = notificationService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _hubContext = hubContext;
            _dbContext = dbContext;
        }

        public async Task SendOverdueShipmentAlertsAsync()
        {
            var shipments = (await _shipmentRepository.GetAllAsync()).ToList();
            var now = DateTime.UtcNow;

            var overdue = shipments
                .Where(s => s.ETA.HasValue && s.ETA.Value < now && s.Status != Core.Entities.ShipmentStatus.Delivered && s.Status != Core.Entities.ShipmentStatus.Cancelled)
                .ToList();

            if (!overdue.Any())
                return;

            var message = $"{overdue.Count} overdue shipments detected at {now:O}.";
            await _notificationService.SendEmailAsync("ops@freightsystem.local", "Overdue shipments alert", message);
            await _notificationService.SendSmsAsync("+201000000001", message);

            await SendSlackNotificationAsync(overdue, message);

            await _hubContext.Clients.All.SendAsync("OverdueAlert", new
            {
                Count = overdue.Count,
                Message = message,
                Time = now
            });
        }

        public async Task AutoPopulateDelayHistoryForMissedEtaAsync()
        {
            var shipments = (await _shipmentRepository.GetAllAsync()).ToList();
            var now = DateTime.UtcNow;

            // retention from config (default 90 days)
            var timeWindowDays = _configuration.GetValue<int>("Monitoring:DelayHistoryRetentionDays", 90);
            var clusterWindowDays = _configuration.GetValue<int>("Monitoring:ClusterHistoryRetentionDays", 90);

            var delayRetention = now.AddDays(-timeWindowDays);
            var clusterRetention = now.AddDays(-clusterWindowDays);

            var oldDelay = _dbContext.DelayHistories.Where(x => x.CreatedAt < delayRetention);
            _dbContext.DelayHistories.RemoveRange(oldDelay);

            var oldClusters = _dbContext.DelayAnomalyClusterHistories.Where(x => x.CreatedAt < clusterRetention);
            _dbContext.DelayAnomalyClusterHistories.RemoveRange(oldClusters);

            await _dbContext.SaveChangesAsync();

            var missed = shipments
                .Where(s => s.ETA.HasValue && s.ETA.Value < now && s.Status != Core.Entities.ShipmentStatus.Delivered && s.Status != Core.Entities.ShipmentStatus.Cancelled)
                .ToList();

            foreach (var shipment in missed)
            {
                var existing = await _dbContext.DelayHistories
                    .Where(x => x.ShipmentId == shipment.Id && x.RecordDate == now.Date)
                    .FirstOrDefaultAsync();

                if (existing != null) continue;

                var durationHours = shipment.ETD.HasValue ? (now - shipment.ETD.Value).TotalHours : 0;
                var delayMinutes = (now - shipment.ETA.Value).TotalMinutes;

                var record = new Core.Entities.DelayHistory
                {
                    ShipmentId = shipment.Id,
                    ETD = shipment.ETD,
                    ETA = shipment.ETA,
                    ActualDeparture = shipment.ETD ?? now,
                    ActualArrival = now,
                    DurationHours = durationHours,
                    DelayMinutes = delayMinutes,
                    Status = shipment.Status.ToString(),
                    TenantId = shipment.TenantId,
                    CreatedAt = now
                };

                _dbContext.DelayHistories.Add(record);
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task SendSlackNotificationAsync(IEnumerable<Shipment> overdue, string summary)
        {
            var url = _configuration.GetValue<string>("Notifications:SlackWebhookUrl");
            if (string.IsNullOrWhiteSpace(url))
                return;

            var payload = new
            {
                text = $"[FreightSystem] {summary}",
                attachments = overdue.Select(s => new
                {
                    fallback = $"Shipment {s.TrackingNumber} overdue",
                    color = "#FF0000",
                    title = s.TrackingNumber,
                    fields = new[]
                    {
                        new { title = "Status", value = s.Status.ToString(), @short = true },
                        new { title = "ETA", value = s.ETA?.ToString("u") ?? "N/A", @short = true },
                        new { title = "CustomerId", value = s.CustomerId.ToString(), @short = true }
                    }
                }).ToArray()
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                await client.PostAsJsonAsync(url, payload);
            }
            catch
            {
                // ignore Slack errors
            }
        }
    }
}

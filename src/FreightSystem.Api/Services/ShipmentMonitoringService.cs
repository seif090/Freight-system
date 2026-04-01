using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;

namespace FreightSystem.Api.Services
{
    public class ShipmentMonitoringService
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly INotificationService _notificationService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ShipmentMonitoringService(
            IShipmentRepository shipmentRepository,
            INotificationService notificationService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _shipmentRepository = shipmentRepository;
            _notificationService = notificationService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
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

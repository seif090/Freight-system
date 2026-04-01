using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;

namespace FreightSystem.Api.Services
{
    public class ShipmentMonitoringService
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly INotificationService _notificationService;

        public ShipmentMonitoringService(IShipmentRepository shipmentRepository, INotificationService notificationService)
        {
            _shipmentRepository = shipmentRepository;
            _notificationService = notificationService;
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
        }
    }
}

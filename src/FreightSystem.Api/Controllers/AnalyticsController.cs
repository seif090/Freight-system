using FreightSystem.Api.Filters;
using FreightSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreightSystem.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "SalesPolicy")]
public class AnalyticsController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;

    public AnalyticsController(IShipmentRepository shipmentRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    [HttpGet("summary")]
    [XDescription("Get shipment summary with trend and prediction.", "الحصول على ملخص الشحنات مع التوجه والتنبؤ.")]
    public async Task<IActionResult> GetSummary()
    {
        var shipments = (await _shipmentRepository.GetAllAsync()).ToList();
        var total = shipments.Count;
        var overdue = shipments.Count(s => s.ETA.HasValue && s.ETA.Value < DateTime.UtcNow && s.Status != Core.Entities.ShipmentStatus.Delivered && s.Status != Core.Entities.ShipmentStatus.Cancelled);
        var highPriority = shipments.Count(s => s.Priority == Core.Entities.ShipmentPriority.High || s.Priority == Core.Entities.ShipmentPriority.Critical);

        var predictedDelay = overdue > 0 ? (double)overdue / total * 100 : 0;

        return Ok(new
        {
            Total = total,
            Overdue = overdue,
            HighPriority = highPriority,
            PredictedDelayPercent = Math.Round(predictedDelay, 1)
        });
    }
}

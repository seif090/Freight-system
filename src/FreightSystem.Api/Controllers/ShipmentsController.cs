using FreightSystem.Api.Filters;
using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Hangfire;
using System.Globalization;
using System.IO;

namespace FreightSystem.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IHubContext<LiveTrackingHub> _hubContext;
    private readonly INotificationService _notificationService;

    public ShipmentsController(IShipmentRepository shipmentRepository, IHubContext<LiveTrackingHub> hubContext, INotificationService notificationService)
    {
        _shipmentRepository = shipmentRepository;
        _hubContext = hubContext;
        _notificationService = notificationService;
    }

    [HttpGet]
    [Authorize(Policy = "SalesPolicy")]
    [XDescription("Retrieve all tenant shipments for the current user.", "استرجاع جميع شحنات المستأجر للمستخدم الحالي.")]
    public async Task<IActionResult> GetAll([FromServices] ITenantContext tenantContext)
    {
        var tenantId = tenantContext.TenantId;
        var shipments = await _shipmentRepository.GetAllByTenantAsync(tenantId);
        return Ok(shipments);
    }

    [HttpGet("route-suggestion")]
    [Authorize(Policy = "OperationPolicy")]
    [XDescription("Recommend optimized route based on origin/destination.", "اقتراح مسار محسّن بناءً على نقطة الانطلاق/الوصول.")]
    public IActionResult GetRouteSuggestions([FromQuery] string origin, [FromQuery] string destination)
    {
        if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
            return BadRequest("origin and destination required");

        // Mock route optimization
        var result = new
        {
            origin,
            destination,
            distanceKm = 1380,
            estimatedTimeHours = 23.5,
            riskLevel = "Medium",
            recommendedSteps = new[]
            {
                "Port departure: check customs clearance",
                "Use inland ferries for mountain region",
                "Avoid high congestion corridors during daytime"
            }
        };

        return Ok(result);
    }

    [HttpPost("import")]
    [Authorize(Policy = "AdminPolicy")]
    [XDescription("Bulk import shipments from CSV. Headers: TrackingNumber,Type,Mode,Status,PortOfLoading,PortOfDischarge,ETD,ETA,Priority,CustomerId,SupplierId", "استيراد دفعة من ملفات CSV مع الحقول.")]
    public async Task<IActionResult> ImportCsv([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("CSV file is required.");

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        var lineNumber = 0;
        var added = 0;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            lineNumber++;
            if (lineNumber == 1 || string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(',');
            if (parts.Length < 11) continue;

            var shipment = new Shipment
            {
                TrackingNumber = parts[0].Trim(),
                Type = Enum.TryParse<ShipmentType>(parts[1].Trim(), true, out var type) ? type : ShipmentType.Domestic,
                Mode = Enum.TryParse<TransportMode>(parts[2].Trim(), true, out var mode) ? mode : TransportMode.Land,
                Status = Enum.TryParse<ShipmentStatus>(parts[3].Trim(), true, out var status) ? status : ShipmentStatus.Pending,
                PortOfLoading = parts[4].Trim(),
                PortOfDischarge = parts[5].Trim(),
                ETD = DateTime.TryParse(parts[6].Trim(), out var etd) ? etd : (DateTime?)null,
                ETA = DateTime.TryParse(parts[7].Trim(), out var eta) ? eta : (DateTime?)null,
                Priority = Enum.TryParse<ShipmentPriority>(parts[8].Trim(), true, out var priority) ? priority : ShipmentPriority.Normal,
                CustomerId = int.TryParse(parts[9].Trim(), out var cid) ? cid : 0,
                SupplierId = int.TryParse(parts[10].Trim(), out var sid) ? sid : (int?)null,
            };

            await _shipmentRepository.AddAsync(shipment);
            added++;
        }

        return Ok(new { imported = added });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);
        if (shipment is null)
            return NotFound();

        return Ok(shipment);
    }

    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> Create([FromBody] Shipment shipment)
    {
        if (shipment is null)
            return BadRequest();

        shipment.TrackingNumber = (shipment.TrackingNumber ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(shipment.TrackingNumber))
        {
            shipment.TrackingNumber = $"TRK-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }

        await _shipmentRepository.AddAsync(shipment);

        // Real-time update to SignalR clients
        await _hubContext.Clients.All.SendAsync("ShipmentCreated", new
        {
            shipment.Id,
            shipment.TrackingNumber,
            shipment.Status
        });

        // schedule a notification job
        BackgroundJob.Enqueue(() => Console.WriteLine($"New shipment created: {shipment.TrackingNumber}"));
        BackgroundJob.Enqueue<INotificationService>(x => x.SendEmailAsync("ops@freightsystem.local", "Shipment Created", $"Shipment {shipment.TrackingNumber} was created."));
        BackgroundJob.Enqueue<INotificationService>(x => x.SendSmsAsync("+201000000001", $"New shipment created: {shipment.TrackingNumber}"));

        return CreatedAtAction(nameof(GetById), new { id = shipment.Id }, shipment);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "OperationPolicy")]
    public async Task<IActionResult> Update(int id, [FromBody] Shipment request)
    {
        var existing = await _shipmentRepository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        existing.Type = request.Type;
        existing.Mode = request.Mode;
        existing.Status = request.Status;
        existing.PortOfLoading = request.PortOfLoading;
        existing.PortOfDischarge = request.PortOfDischarge;
        existing.ETD = request.ETD;
        existing.ETA = request.ETA;
        existing.ContainerType = request.ContainerType;
        existing.VesselOrFlightNumber = request.VesselOrFlightNumber;
        existing.CustomerId = request.CustomerId;
        existing.SupplierId = request.SupplierId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _shipmentRepository.UpdateAsync(existing);

        // Live tracking notification
        await _hubContext.Clients.Group($"Shipment_{existing.Id}").SendAsync("ShipmentUpdated", new
        {
            existing.Id,
            existing.Status,
            existing.ETA,
            existing.ETD
        });

        BackgroundJob.Enqueue(() => Console.WriteLine($"Shipment updated: {existing.TrackingNumber} status {existing.Status}"));
        BackgroundJob.Enqueue<INotificationService>(x => x.SendEmailAsync("ops@freightsystem.local", "Shipment Updated", $"Shipment {existing.TrackingNumber} status changed to {existing.Status}."));
        BackgroundJob.Enqueue<INotificationService>(x => x.SendSmsAsync("+201000000001", $"Shipment {existing.TrackingNumber} status: {existing.Status}"));

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _shipmentRepository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        await _shipmentRepository.DeleteAsync(existing);
        return NoContent();
    }
}

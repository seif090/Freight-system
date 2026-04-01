using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Hangfire;

namespace FreightSystem.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IHubContext<LiveTrackingHub> _hubContext;

    public ShipmentsController(IShipmentRepository shipmentRepository, IHubContext<LiveTrackingHub> hubContext)
    {
        _shipmentRepository = shipmentRepository;
        _hubContext = hubContext;
    }

    [HttpGet]
    [Authorize(Policy = "SalesPolicy")]
    public async Task<IActionResult> GetAll()
    {
        var shipments = await _shipmentRepository.GetAllAsync();
        return Ok(shipments);
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

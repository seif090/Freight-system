using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FreightSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;

    public ShipmentsController(IShipmentRepository shipmentRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    [HttpGet]
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
    public async Task<IActionResult> Create([FromBody] Shipment shipment)
    {
        if (shipment is null)
            return BadRequest();

        shipment.TrackingNumber = shipment.TrackingNumber?.Trim();
        if (string.IsNullOrWhiteSpace(shipment.TrackingNumber))
        {
            shipment.TrackingNumber = $"TRK-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }

        await _shipmentRepository.AddAsync(shipment);
        return CreatedAtAction(nameof(GetById), new { id = shipment.Id }, shipment);
    }

    [HttpPut("{id:int}")]
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
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _shipmentRepository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        await _shipmentRepository.DeleteAsync(existing);
        return NoContent();
    }
}

using FreightSystem.Application.Interfaces;
using FreightSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreightSystem.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AdvancedOperationsController : ControllerBase
    {
        private readonly FreightDbContext _dbContext;
        private readonly IRouteOptimizationService _routeOptimizationService;

        public AdvancedOperationsController(FreightDbContext dbContext, IRouteOptimizationService routeOptimizationService)
        {
            _dbContext = dbContext;
            _routeOptimizationService = routeOptimizationService;
        }

        [HttpGet("vehicles")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> GetVehicles([FromQuery] string? tenantId)
        {
            var vehicles = await _dbContext.Vehicles
                .Where(v => string.IsNullOrEmpty(tenantId) || v.TenantId == tenantId)
                .Include(v => v.MaintenanceEvents)
                .ToListAsync();
            return Ok(vehicles);
        }

        [HttpPost("vehicles")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AddVehicle([FromBody] Core.Entities.Vehicle vehicle)
        {
            if (vehicle is null) return BadRequest();
            vehicle.TenantId ??= "default";
            _dbContext.Vehicles.Add(vehicle);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
        }

        [HttpGet("vehicles/{id:int}")]
        [Authorize(Policy = "SalesPolicy")]
        public async Task<IActionResult> GetVehicle(int id)
        {
            var v = await _dbContext.Vehicles.Include(x => x.MaintenanceEvents).FirstOrDefaultAsync(x => x.Id == id);
            return v == null ? NotFound() : Ok(v);
        }

        [HttpPost("vehicles/{id:int}/maintenance")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> AddMaintenance(int id, [FromBody] Core.Entities.MaintenanceEvent maintenance)
        {
            var vehicle = await _dbContext.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();
            maintenance.VehicleId = id;
            _dbContext.MaintenanceEvents.Add(maintenance);
            vehicle.LastInspection = DateTime.UtcNow;
            vehicle.NextInspectionDue = DateTime.UtcNow.AddMonths(3);
            await _dbContext.SaveChangesAsync();
            return Ok(maintenance);
        }

        [HttpGet("vehicles/maintenance-risk")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> GetMaintenanceRisk([FromQuery] string tenantId = "default")
        {
            var risky = await _routeOptimizationService.EvaluateMaintenanceRiskAsync(tenantId);
            return Ok(risky);
        }

        [HttpPost("shipments/{shipmentId:int}/optimize-route")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> OptimizeRoute(int shipmentId, [FromBody] IEnumerable<Core.Entities.RouteSegment> plannedSegments)
        {
            var result = await _routeOptimizationService.OptimizeRouteAsync(shipmentId, plannedSegments);
            return Ok(result);
        }
    }
}

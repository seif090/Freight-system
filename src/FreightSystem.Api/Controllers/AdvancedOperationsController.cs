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

        [HttpPatch("shipments/{shipmentId:int}/dispatch")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> DispatchRoute(int shipmentId, [FromBody] DispatchRequest request)
        {
            var shipment = await _dbContext.Shipments.FindAsync(shipmentId);
            if (shipment == null) return NotFound();

            var dispatchRecord = new Core.Entities.DispatchAction
            {
                ShipmentId = shipmentId,
                Instruction = request.Instruction ?? "Manual dispatch command",
                RoutePreviewUrl = request.RoutePreviewUrl ?? string.Empty,
                RouteGeoJson = request.RouteGeoJson ?? string.Empty,
                Priority = request.Priority ?? "High",
                Dispatched = request.MarkDispatched,
                DispatchedAt = request.MarkDispatched ? DateTime.UtcNow : null,
                TenantId = shipment.TenantId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.DispatchActions.Add(dispatchRecord);
            await _dbContext.SaveChangesAsync();

            return Ok(dispatchRecord);
        }

        [HttpGet("dispatch-actions")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> GetDispatchActions(int page = 1, int pageSize = 20)
        {
            var actions = await _dbContext.DispatchActions
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var total = await _dbContext.DispatchActions.CountAsync();
            return Ok(new { actions, page, pageSize, total });
        }

        [HttpPatch("dispatch-actions/{id:int}/undo")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> UndoDispatch(int id)
        {
            var action = await _dbContext.DispatchActions.FindAsync(id);
            if (action == null) return NotFound();

            action.Dispatched = false;
            action.Priority = "High";
            action.DispatchedAt = null;
            await _dbContext.SaveChangesAsync();

            return Ok(action);
        }
    }

    public class DispatchRequest
    {
        public string? Instruction { get; set; }
        public string? RoutePreviewUrl { get; set; }
        public string? RouteGeoJson { get; set; }
        public string? Priority { get; set; }
        public bool MarkDispatched { get; set; }
    }
}

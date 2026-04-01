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
    public class AdvancedAnalyticsController : ControllerBase
    {
        private readonly FreightDbContext _dbContext;
        private readonly IGeoService _geoService;
        private readonly IMLService _mlService;
        private readonly IEventBus _eventBus;

        public AdvancedAnalyticsController(FreightDbContext dbContext, IGeoService geoService, IMLService mlService, IEventBus eventBus)
        {
            _dbContext = dbContext;
            _geoService = geoService;
            _mlService = mlService;
            _eventBus = eventBus;
        }

        [HttpPost("geofences")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> AddGeofence([FromBody] Core.Entities.Geofence geofence)
        {
            if (geofence is null) return BadRequest();
            geofence.CreatedAt = DateTime.UtcNow;
            _dbContext.Geofences.Add(geofence);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGeofence), new { id = geofence.Id }, geofence);
        }

        [HttpGet("geofences")]
        [Authorize(Policy = "SalesPolicy")]
        public async Task<IActionResult> GetGeofences()
        {
            var geofences = await _dbContext.Geofences.OrderByDescending(g => g.CreatedAt).ToListAsync();
            return Ok(geofences);
        }

        [HttpGet("geofences/{id:int}")]
        [Authorize(Policy = "SalesPolicy")]
        public async Task<IActionResult> GetGeofence(int id)
        {
            var geofence = await _dbContext.Geofences.FindAsync(id);
            return geofence == null ? NotFound() : Ok(geofence);
        }

        [HttpGet("shipments/{shipmentId:int}/geofence-check")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> GeofenceCheck(int shipmentId)
        {
            var shipment = await _dbContext.Shipments.FindAsync(shipmentId);
            if (shipment == null || !shipment.CurrentLatitude.HasValue || !shipment.CurrentLongitude.HasValue) return NotFound();

            var result = await _geoService.CheckGeofenceAsync(shipment.CurrentLatitude.Value, shipment.CurrentLongitude.Value, shipment.TenantId);
            return Ok(result);
        }

        [HttpPost("shipments/{shipmentId:int}/segments")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> AddRouteSegment(int shipmentId, [FromBody] Core.Entities.RouteSegment segment)
        {
            var shipment = await _dbContext.Shipments.FindAsync(shipmentId);
            if (shipment == null) return NotFound();

            segment.ShipmentId = shipmentId;
            segment.TenantId = shipment.TenantId;
            _dbContext.RouteSegments.Add(segment);
            await _dbContext.SaveChangesAsync();

            var etaDetail = await _geoService.CalculateSegmentEtaAsync(segment, shipment);

            var etaPrediction = await _mlService.PredictETAAsync(shipment, new[] { segment });

            await _eventBus.PublishAsync("route-segment-added", new
            {
                ShipmentId = shipment.Id,
                shipment.TrackingNumber,
                RouteSegmentId = segment.Id,
                segment.SegmentOrder,
                etaPrediction.PredictedETA,
                etaPrediction.Confidence,
                etaPrediction.PredictedDelayMinutes
            });

            return CreatedAtAction(nameof(GetRouteSegment), new { id = segment.Id }, new { segment, etaDetail, etaPrediction });
        }

        [HttpGet("segments/{id:int}")]
        [Authorize(Policy = "SalesPolicy")]
        public async Task<IActionResult> GetRouteSegment(int id)
        {
            var segment = await _dbContext.RouteSegments.FindAsync(id);
            return segment == null ? NotFound() : Ok(segment);
        }

        [HttpPost("warehouse/snapshot")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> CreateWarehouseSnapshot()
        {
            var shipments = await _dbContext.Shipments.Include(s => s.Customer).ToListAsync();
            var facts = shipments.Select(s => new Core.Entities.WarehouseShipmentFact
            {
                ShipmentId = s.Id,
                TrackingNumber = s.TrackingNumber,
                RouteKey = $"{s.PortOfLoading}->{s.PortOfDischarge}",
                Status = s.Status.ToString(),
                Origin = s.PortOfLoading,
                Destination = s.PortOfDischarge,
                ETD = s.ETD,
                ETA = s.ETA,
                TenantId = s.TenantId,
                IsDelayAnomaly = false,
                FactDate = DateTime.UtcNow
            }).ToList();

            _dbContext.WarehouseShipmentFacts.AddRange(facts);
            await _dbContext.SaveChangesAsync();

            return Ok(new { inserted = facts.Count });
        }

        [HttpGet("warehouse/facts")]
        [Authorize(Policy = "SalesPolicy")]
        public async Task<IActionResult> GetWarehouseFacts([FromQuery] int limit = 200)
        {
            var facts = await _dbContext.WarehouseShipmentFacts.OrderByDescending(f => f.FactDate).Take(limit).ToListAsync();
            return Ok(facts);
        }

        [HttpPost("shipments/{shipmentId:int}/delay-anomaly-check")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> DelayAnomalyCheck(int shipmentId)
        {
            var shipment = await _dbContext.Shipments.FindAsync(shipmentId);
            if (shipment == null) return NotFound();

            var response = await _mlService.AnalyzeDelayAsync(shipment);
            return Ok(response);
        }

        [HttpPost("shipments/{shipmentId:int}/stream-history")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> StreamHistory(int shipmentId)
        {
            var history = await _dbContext.ShipmentLocationHistory
                .Where(x => x.ShipmentId == shipmentId)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();

            var success = await _mlService.StreamRouteDataAsync(shipmentId, history);
            return Ok(new { success });
        }
    }
}

using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreightSystem.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelematicsService _telematicsService;
        private readonly ITrafficService _trafficService;
        private readonly IAiTrainingService _aiTrainingService;

        public TelemetryController(ITelematicsService telematicsService, ITrafficService trafficService, IAiTrainingService aiTrainingService)
        {
            _telematicsService = telematicsService;
            _trafficService = trafficService;
            _aiTrainingService = aiTrainingService;
        }

        [HttpPost("ingest")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> Ingest([FromBody] TelematicsData data)
        {
            data.Timestamp = DateTime.UtcNow;
            await _telematicsService.AddDataAsync(data);
            return Ok(new { status = "ingested", data.Id });
        }

        [HttpGet("shipment/{shipmentId:int}")]
        [Authorize(Policy = "SalesPolicy")]
        public async Task<IActionResult> GetByShipment(int shipmentId)
        {
            var records = await _telematicsService.GetByShipmentAsync(shipmentId);
            return Ok(records);
        }

        [HttpGet("traffic")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> GetTraffic([FromQuery] double lat, [FromQuery] double lon)
        {
            var forecast = await _trafficService.GetTrafficForecastAsync(lat, lon);
            return Ok(new { forecast, latitude = lat, longitude = lon });
        }

        [HttpPost("ai-train")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> QueueTraining()
        {
            var result = await _aiTrainingService.QueueTrainingAsync();
            return Ok(new { result });
        }

        [HttpGet("ai-training-status")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> GetTrainingStatus()
        {
            var status = await _aiTrainingService.GetTrainingStatusAsync();
            return Ok(new { status });
        }
    }
}

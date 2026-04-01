using FreightSystem.Api.Filters;
using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Settings;
using FreightSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FreightSystem.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "SalesPolicy")]
public class AnalyticsController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly FreightDbContext _dbContext;
    private readonly LlmSettings _llmSettings;

    public AnalyticsController(IShipmentRepository shipmentRepository, FreightDbContext dbContext, IOptions<LlmSettings> llmOptions)
    {
        _shipmentRepository = shipmentRepository;
        _dbContext = dbContext;
        _llmSettings = llmOptions.Value;
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

    [HttpGet("slo")]
    [XDescription("Get SLO status and breached shipments.", "الحصول على حالة SLO والشحنات التي تنتهكها.")]
    public async Task<IActionResult> GetSlo([FromQuery] double thresholdPercent = 5)
    {
        var shipments = (await _shipmentRepository.GetAllAsync()).ToList();
        var total = Math.Max(1, shipments.Count);
        var overdue = shipments.Count(s => s.ETA.HasValue && s.ETA.Value < DateTime.UtcNow && s.Status != Core.Entities.ShipmentStatus.Delivered && s.Status != Core.Entities.ShipmentStatus.Cancelled);
        var currentRate = overdue * 100.0 / total;

        var slo = new
        {
            Threshold = thresholdPercent,
            CurrentDelayRate = Math.Round(currentRate, 2),
            IsBreached = currentRate > thresholdPercent,
            BreachedShipments = shipments.Where(s => s.ETA.HasValue && s.ETA.Value < DateTime.UtcNow && s.Status != Core.Entities.ShipmentStatus.Delivered && s.Status != Core.Entities.ShipmentStatus.Cancelled)
                .Select(s => new { s.Id, s.TrackingNumber, s.Status, s.ETA, s.Priority })
                .Take(50)
                .ToList()
        };

        return Ok(slo);
    }

    [HttpGet("llm-spend")]
    [XDescription("Get LLM spend logs by date range","استرداد سجلات الإنفاق على LLM بحسب النطاق الزمني")]
    public async Task<IActionResult> GetLlmSpend([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var query = _dbContext.LlmSpendLogs.AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(x => x.Timestamp >= from.Value.ToUniversalTime());
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.Timestamp <= to.Value.ToUniversalTime());
        }

        var entries = await query.OrderByDescending(x => x.Timestamp).ToListAsync();

        var spendSummary = new
        {
            TotalEntries = entries.Count,
            TotalTokenUsage = entries.Sum(x => x.TokenUsage),
            TotalCostUsd = entries.Sum(x => x.EstimatedCostUsd),
            ByProvider = entries.GroupBy(x => x.Provider).Select(g => new
            {
                Provider = g.Key,
                Entries = g.Count(),
                TokenUsage = g.Sum(x => x.TokenUsage),
                CostUsd = g.Sum(x => x.EstimatedCostUsd)
            }).ToList(),
            ByDay = entries.GroupBy(x => x.Timestamp.Date).Select(g => new
            {
                Date = g.Key,
                Entries = g.Count(),
                TokenUsage = g.Sum(x => x.TokenUsage),
                CostUsd = g.Sum(x => x.EstimatedCostUsd)
            }).OrderBy(x => x.Date).ToList()
        };

        return Ok(new { entries, spendSummary });
    }

    [HttpGet("llm-model-pricing")]
    [XDescription("Get LLM model cost per token table","الحصول على جدول تكلفة نموذج LLM لكل توكن")]
    public IActionResult GetLlmModelPricing()
    {
        return Ok(new
        {
            _llmSettings.ModelCostPerToken,
            DefaultProvider = _llmSettings.Provider,
            OpenAiModel = _llmSettings.OpenAiModel,
            ClaudeModel = _llmSettings.ClaudeModel
        });
    }
}

using ClosedXML.Excel;
using FreightSystem.Api.Filters;
using FreightSystem.Application.Interfaces;
using FreightSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text;

namespace FreightSystem.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly FreightDbContext _dbContext;
    private readonly IMLService _mlService;

    public ReportsController(IShipmentRepository shipmentRepository, ICustomerRepository customerRepository, FreightDbContext dbContext, IMLService mlService)
    {
        _shipmentRepository = shipmentRepository;
        _customerRepository = customerRepository;
        _dbContext = dbContext;
        _mlService = mlService;
    }

    [HttpGet("dashboard")]
    [Authorize(Policy = "SalesPolicy")]
    [XDescription("Get dashboard KPI data for shipments and customers.", "الحصول على بيانات لوحة القيادة للشحنات والعملاء.")]
    public async Task<IActionResult> GetDashboard()
    {
        var shipments = (await _shipmentRepository.GetAllAsync()).ToList();
        var customers = (await _customerRepository.GetAllAsync()).ToList();

        var shipmentsPerStatus = shipments
            .GroupBy(x => x.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var shipmentsPerMode = shipments
            .GroupBy(x => x.Mode.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var monthlyShipmentCount = shipments
            .GroupBy(x => x.CreatedAt.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Count());

        var topCustomers = customers
            .OrderByDescending(c => c.Invoices?.Sum(inv => inv.Amount + inv.VAT) ?? 0)
            .Take(5)
            .Select(c => new { c.Id, c.Name, TotalInvoiced = c.Invoices?.Sum(i => i.Amount + i.VAT) ?? 0 });

        return Ok(new
        {
            TotalShipments = shipments.Count,
            TotalCustomers = customers.Count,
            ShipmentsPerStatus = shipmentsPerStatus,
            ShipmentsPerMode = shipmentsPerMode,
            MonthlyShipmentCount = monthlyShipmentCount,
            TopCustomers = topCustomers
        });
    }

    [HttpGet("overdue")]
    [Authorize(Policy = "OperationPolicy")]
    [XDescription("Get overdue shipments that missed ETA and are not delivered or cancelled.", "جلب الشحنات المتأخرة التي تجاوزت ETA ولا تزال ليست مُسلمة أو ملغاة.")]
    public async Task<IActionResult> GetOverdueShipments()
    {
        var shipments = (await _shipmentRepository.GetAllAsync()).ToList();
        var now = DateTime.UtcNow;

        var overdue = shipments
            .Where(s => s.ETA.HasValue && s.ETA.Value < now && s.Status != Core.Entities.ShipmentStatus.Delivered && s.Status != Core.Entities.ShipmentStatus.Cancelled)
            .ToList();

        return Ok(new
        {
            Count = overdue.Count,
            OverdueShipments = overdue
        });
    }

    [HttpGet("top-customers")]
    [Authorize(Policy = "SalesPolicy")]
    [XDescription("Get top customers by revenue.", "جلب أفضل العملاء حسب الإيرادات.")]
    public async Task<IActionResult> GetTopCustomers()
    {
        var customers = (await _customerRepository.GetAllAsync()).ToList();

        var topCustomers = customers
            .Select(c => new { c.Id, c.Name, TotalInvoiced = c.Invoices?.Sum(i => i.Amount + i.VAT) ?? 0 })
            .OrderByDescending(c => c.TotalInvoiced)
            .Take(10);

        return Ok(topCustomers);
    }

        [HttpGet("financial/summary")]
        [Authorize(Policy = "SalesPolicy")]
        [XDescription("Get financial performance summary for invoices.", "الحصول على ملخص الأداء المالي للفواتير.")]
        public async Task<IActionResult> GetFinancialSummary()
        {
            var invoices = await _dbContext.Invoices.Include(i => i.Customer).ToListAsync();
            var now = DateTime.UtcNow;

            var totalInvoiced = invoices.Sum(i => i.Amount + i.VAT);
            var paidAmount = invoices.Where(i => i.Status == Core.Entities.InvoiceStatus.Paid).Sum(i => i.Amount + i.VAT);
            var outstanding = invoices.Where(i => i.Status != Core.Entities.InvoiceStatus.Paid && i.Status != Core.Entities.InvoiceStatus.Cancelled).Sum(i => i.Amount + i.VAT);
            var overdueInvoices = invoices.Where(i => i.DueDate.HasValue && i.DueDate.Value < now && i.Status != Core.Entities.InvoiceStatus.Paid && i.Status != Core.Entities.InvoiceStatus.Cancelled).ToList();
            var overdueAmount = overdueInvoices.Sum(i => i.Amount + i.VAT);
            var avgOverdueDays = overdueInvoices.Any() ? overdueInvoices.Average(i => (now - i.DueDate!.Value).TotalDays) : 0;
            var customerCreditUsage = invoices
                .GroupBy(i => i.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    CustomerName = g.First().Customer?.Name ?? "Unknown",
                    TotalBilled = g.Sum(i => i.Amount + i.VAT),
                    Unpaid = g.Where(i => i.Status != Core.Entities.InvoiceStatus.Paid && i.Status != Core.Entities.InvoiceStatus.Cancelled).Sum(i => i.Amount + i.VAT)
                });

            return Ok(new
            {
                TotalInvoiceCount = invoices.Count,
                TotalInvoiced = totalInvoiced,
                PaidAmount = paidAmount,
                OutstandingAmount = outstanding,
                OverdueCount = overdueInvoices.Count,
                OverdueAmount = overdueAmount,
                AvgOverdueDays = avgOverdueDays,
                CustomerCreditUsage = customerCreditUsage.OrderByDescending(c => c.Unpaid).Take(20)
            });
        }

        [HttpGet("financial/aging")]
        [Authorize(Policy = "SalesPolicy")]
        [XDescription("Get invoice aging buckets for receivables.", "الحصول على دلاء الشيخوخة للفواتير.")]
        public async Task<IActionResult> GetInvoiceAging()
        {
            var now = DateTime.UtcNow;
            var invoices = await _dbContext.Invoices
                .Where(i => i.Status != Core.Entities.InvoiceStatus.Paid && i.Status != Core.Entities.InvoiceStatus.Cancelled && i.DueDate.HasValue)
                .ToListAsync();

            var buckets = new Dictionary<string, decimal>
            {
                ["current"] = 0,
                ["1-30"] = 0,
                ["31-60"] = 0,
                ["61-90"] = 0,
                ["91+"] = 0
            };

            foreach (var invoice in invoices)
            {
                var daysPast = (now - invoice.DueDate!.Value).TotalDays;
                var amount = invoice.Amount + invoice.VAT;
                if (daysPast <= 0) buckets["current"] += amount;
                else if (daysPast <= 30) buckets["1-30"] += amount;
                else if (daysPast <= 60) buckets["31-60"] += amount;
                else if (daysPast <= 90) buckets["61-90"] += amount;
                else buckets["91+"] += amount;
            }

            return Ok(new { AgingBuckets = buckets, TotalOutstanding = invoices.Sum(x => x.Amount + x.VAT) });
        }

        [HttpPost("invoices/{invoiceId:int}/mark-paid")]
        [Authorize(Policy = "SalesPolicy")]
        [XDescription("Mark an invoice as paid.", "وضع علامة مدفوعة على الفاتورة.")]
        public async Task<IActionResult> MarkInvoicePaid(int invoiceId)
        {
            var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
            if (invoice == null) return NotFound();

            invoice.Status = Core.Entities.InvoiceStatus.Paid;
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, invoiceId, status = invoice.Status.ToString() });
        }

    [HttpGet("delay-risk-forecast")]
    [Authorize(Policy = "OperationPolicy")]
    [XDescription("Predict delayed shipments using simple heuristic.", "توقع الشحنات المتأخرة باستخدام منهجية بسيطة.")]
    public async Task<IActionResult> GetDelayRiskForecast()
    {
        var shipments = (await _shipmentRepository.GetAllAsync()).ToList();
        var now = DateTime.UtcNow;

        var riskScores = shipments.Select(s => new
        {
            s.Id,
            s.TrackingNumber,
            s.Status,
            s.ETA,
            s.ETD,
            DaysToETA = s.ETA.HasValue ? (s.ETA.Value - now).TotalDays : double.MaxValue,
            RiskScore = s.ETA.HasValue
                ? Math.Max(0, 100 - Math.Min(120, (s.ETA.Value - now).TotalHours))
                : 100
        }).OrderByDescending(x => x.RiskScore).Take(20);

        return Ok(new
        {
            AsOf = now,
            HighRisk = riskScores.Where(x => x.RiskScore >= 70),
            MediumRisk = riskScores.Where(x => x.RiskScore >= 40 && x.RiskScore < 70),
            LowRisk = riskScores.Where(x => x.RiskScore < 40)
        });
    }

    [HttpGet("delay-regression")]
    [Authorize(Policy = "OperationPolicy")]
    [XDescription("Get linear regression-based delay forecast.", "الحصول على توقع التأخير باستخدام الانحدار الخطي.")]
    public async Task<IActionResult> GetDelayRegression([FromQuery] int sampleSize = 100)
    {
        var shipments = await _dbContext.Shipments
            .Where(s => s.Status == Core.Entities.ShipmentStatus.Delivered && s.ETA.HasValue && s.ETD.HasValue && s.UpdatedAt.HasValue)
            .OrderByDescending(s => s.UpdatedAt)
            .Take(sampleSize)
            .ToListAsync();

        var regression = await _mlService.PredictDelayRegressionAsync(shipments);

        return Ok(new
        {
            regression.Slope,
            regression.Intercept,
            regression.RSquared,
            regression.ForecastDelayMinutes,
            regression.SampleSize,
            Samples = regression.Samples
        });
    }

    [HttpGet("delay-history")]
    [Authorize(Policy = "OperationPolicy")]
    [XDescription("Get delay history records.", "الحصول على سجلات تاريخ التأخير.")]
    public async Task<IActionResult> GetDelayHistory([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int? shipmentId = null, [FromQuery] int limit = 1000)
    {
        var query = _dbContext.DelayHistories.AsQueryable();

        if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value.ToUniversalTime());
        if (to.HasValue) query = query.Where(x => x.CreatedAt <= to.Value.ToUniversalTime());
        if (shipmentId.HasValue) query = query.Where(x => x.ShipmentId == shipmentId.Value);

        var history = await query.OrderByDescending(x => x.CreatedAt).Take(limit).ToListAsync();

        return Ok(new { count = history.Count, history });
    }

    [HttpGet("delay-history/download")]
    [Authorize(Policy = "OperationPolicy")]
    [XDescription("Download delay history as CSV.", "تحميل سجل التأخير بتنسيق CSV.")]
    public async Task<IActionResult> DownloadDelayHistory([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var query = _dbContext.DelayHistories.AsQueryable();
        if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value.ToUniversalTime());
        if (to.HasValue) query = query.Where(x => x.CreatedAt <= to.Value.ToUniversalTime());

        var history = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Id,ShipmentId,ETD,ETA,ActualDeparture,ActualArrival,DurationHours,DelayMinutes,Status,TenantId,CreatedAt");
        foreach (var item in history)
        {
            csv.AppendLine($"{item.Id},{item.ShipmentId},{item.ETD:o},{item.ETA:o},{item.ActualDeparture:o},{item.ActualArrival:o},{item.DurationHours},{item.DelayMinutes},{item.Status},{item.TenantId},{item.CreatedAt:o}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", "delay-history.csv");
    }

    [HttpGet("delay-history/anomalies")]
    [Authorize(Policy = "OperationPolicy")]
    [XDescription("Get delay anomaly clusters.", "الحصول على مجموعات العيوب في تأخير الشحنات.")]
    public async Task<IActionResult> GetDelayHistoryAnomalies([FromQuery] double thresholdMinutes = 30)
    {
        var matches = await _dbContext.DelayHistories
            .Where(x => Math.Abs(x.DelayMinutes) >= thresholdMinutes)
            .ToListAsync();

        var clusters = matches.GroupBy(x => x.Status).Select(g => new
        {
            Status = g.Key,
            Count = g.Count(),
            AvgDelay = g.Average(x => x.DelayMinutes),
            MaxDelay = g.Max(x => x.DelayMinutes),
            MinDelay = g.Min(x => x.DelayMinutes)
        }).ToList();

        var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var clusterHistory = new Core.Entities.DelayAnomalyClusterHistory
        {
            WeekStarting = weekStart,
            ThresholdMinutes = thresholdMinutes,
            TotalClusters = clusters.Count,
            TotalMatches = matches.Count,
            AvgDelayMinutes = matches.Any() ? matches.Average(x => x.DelayMinutes) : 0,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.DelayAnomalyClusterHistories.Add(clusterHistory);
        await _dbContext.SaveChangesAsync();

        return Ok(new { thresholdMinutes, totalMatches = matches.Count, clusters, matches, clusterHistory });
    }

    [HttpGet("anomaly-cluster-history")]
    [Authorize(Policy = "OperationPolicy")]
    [XDescription("Get delay anomaly cluster history.", "الحصول على تاريخ مجموعات العيوب في التأخير.")]
    public async Task<IActionResult> GetDelayAnomalyClusterHistory([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var query = _dbContext.DelayAnomalyClusterHistories.AsQueryable();

        if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value.ToUniversalTime());
        if (to.HasValue) query = query.Where(x => x.CreatedAt <= to.Value.ToUniversalTime());

        var results = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();

        return Ok(new { count = results.Count, results });
    }

    [HttpGet("export/shipments")]
    [Authorize(Policy = "SalesPolicy")]
    [XDescription("Export shipments data as CSV or Excel (CSV format).", "تصدير بيانات الشحنات بصيغة CSV أو Excel.")]
    public async Task<IActionResult> ExportShipments([FromQuery] string format = "csv")
    {
        var shipments = (await _shipmentRepository.GetAllAsync()).ToList();

        var csv = new StringBuilder();
        csv.AppendLine("Id,TrackingNumber,Status,Mode,ETD,ETA,CustomerId,CreatedAt");
        foreach (var s in shipments)
        {
            csv.AppendLine($"{s.Id},{s.TrackingNumber},{s.Status},{s.Mode},{s.ETD?.ToString("u") ?? ""},{s.ETA?.ToString("u") ?? ""},{s.CustomerId},{s.CreatedAt:u}");
        }

        if (format.ToLower() == "excel")
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Shipments");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "TrackingNumber";
            ws.Cell(1, 3).Value = "Status";
            ws.Cell(1, 4).Value = "Mode";
            ws.Cell(1, 5).Value = "ETD";
            ws.Cell(1, 6).Value = "ETA";
            ws.Cell(1, 7).Value = "CustomerId";
            ws.Cell(1, 8).Value = "CreatedAt";

            for (int i = 0; i < shipments.Count; i++)
            {
                var s = shipments[i];
                ws.Cell(i + 2, 1).Value = s.Id;
                ws.Cell(i + 2, 2).Value = s.TrackingNumber;
                ws.Cell(i + 2, 3).Value = s.Status.ToString();
                ws.Cell(i + 2, 4).Value = s.Mode.ToString();
                ws.Cell(i + 2, 5).Value = s.ETD?.ToString("u") ?? "";
                ws.Cell(i + 2, 6).Value = s.ETA?.ToString("u") ?? "";
                ws.Cell(i + 2, 7).Value = s.CustomerId;
                ws.Cell(i + 2, 8).Value = s.CreatedAt.ToString("u");
            }

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "shipments.xlsx");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", "shipments.csv");
    }
}


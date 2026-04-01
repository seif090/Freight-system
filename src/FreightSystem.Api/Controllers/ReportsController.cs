using FreightSystem.Api.Filters;
using FreightSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace FreightSystem.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ICustomerRepository _customerRepository;

    public ReportsController(IShipmentRepository shipmentRepository, ICustomerRepository customerRepository)
    {
        _shipmentRepository = shipmentRepository;
        _customerRepository = customerRepository;
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

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var contentType = "text/csv";
        var fileName = format.ToLower() == "excel" ? "shipments.xlsx" : "shipments.csv";
        if (format.ToLower() == "excel")
            contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        return File(bytes, contentType, fileName);
    }
}


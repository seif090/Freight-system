using FreightSystem.Api.Filters;
using FreightSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            .GroupBy(x => x.Status ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        var shipmentsPerMode = shipments
            .GroupBy(x => x.Mode ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        var monthlyShipmentCount = shipments
            .GroupBy(x => x.CreatedAt.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Count());

        return Ok(new
        {
            TotalShipments = shipments.Count,
            TotalCustomers = customers.Count,
            ShipmentsPerStatus = shipmentsPerStatus,
            ShipmentsPerMode = shipmentsPerMode,
            MonthlyShipmentCount = monthlyShipmentCount
        });
    }
}

using FreightSystem.Api.Filters;
using FreightSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreightSystem.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "SalesPolicy")]
public class SearchController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ICustomerRepository _customerRepository;

    public SearchController(IShipmentRepository shipmentRepository, ICustomerRepository customerRepository)
    {
        _shipmentRepository = shipmentRepository;
        _customerRepository = customerRepository;
    }

    [HttpGet("shipments")]
    [XDescription("Search shipments by query (tracking, status, customer, port).", "بحث عن الشحنات بكلمة مفتاحية (رقم التتبع، الحالة، العميل، الميناء).")]
    public async Task<IActionResult> SearchShipments([FromQuery] string q)
    {
        var result = await _shipmentRepository.SearchAsync(q);
        return Ok(result);
    }

    [HttpGet("customers")]
    [XDescription("Search customers by query (name, email, phone, address).", "بحث عن العملاء بكلمة مفتاحية (اسم، بريد إلكتروني، هاتف، عنوان).")]
    public async Task<IActionResult> SearchCustomers([FromQuery] string q)
    {
        var result = await _customerRepository.SearchAsync(q);
        return Ok(result);
    }
}

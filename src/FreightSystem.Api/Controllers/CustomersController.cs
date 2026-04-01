using FreightSystem.Api.Filters;
using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreightSystem.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;

    public CustomersController(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Sales")]
    [XDescription("Retrieve all customers.", "استرجاع جميع العملاء.")]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _customerRepository.GetAllAsync();
        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Sales")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> Create([FromBody] Customer customer)
    {
        if (customer == null)
            return BadRequest();

        await _customerRepository.AddAsync(customer);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id, version = "1.0" }, customer);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "OperationPolicy")]
    public async Task<IActionResult> Update(int id, [FromBody] Customer request)
    {
        var existing = await _customerRepository.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        existing.Name = request.Name;
        existing.Email = request.Email;
        existing.Phone = request.Phone;
        existing.Address = request.Address;
        existing.CreditLimit = request.CreditLimit;
        existing.Balance = request.Balance;

        await _customerRepository.UpdateAsync(existing);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _customerRepository.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await _customerRepository.DeleteAsync(existing);
        return NoContent();
    }
}

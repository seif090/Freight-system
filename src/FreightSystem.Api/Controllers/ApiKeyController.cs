using FreightSystem.Application.Interfaces;
using FreightSystem.Api.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreightSystem.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminPolicy")]
public class ApiKeyController : ControllerBase
{
    private readonly IApiKeyManager _apiKeyManager;

    public ApiKeyController(IApiKeyManager apiKeyManager)
    {
        _apiKeyManager = apiKeyManager;
    }

    [HttpGet]
    [XDescription("List active API keys.", "عرض مفاتيح API النشطة.")]
    public IActionResult GetKeys() => Ok(_apiKeyManager.ListKeys());

    [HttpPost("create")]
    [XDescription("Create a new API key for owner.", "إنشاء مفتاح API جديد للمالك.")]
    public IActionResult Create([FromQuery] string owner)
    {
        if (string.IsNullOrWhiteSpace(owner)) return BadRequest("owner required");
        return Ok(new { apiKey = _apiKeyManager.CreateKey(owner) });
    }
}

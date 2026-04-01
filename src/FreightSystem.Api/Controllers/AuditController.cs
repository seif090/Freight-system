using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreightSystem.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminPolicy")]
public class AuditController : ControllerBase
{
    private readonly FreightDbContext _dbContext;

    public AuditController(FreightDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 20;

        var total = _dbContext.AuditLogs.Count();
        var logs = _dbContext.AuditLogs
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            page,
            pageSize,
            total,
            logs
        });
    }
}

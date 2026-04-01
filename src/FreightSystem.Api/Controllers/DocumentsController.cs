using FreightSystem.Infrastructure.Persistence;
using FreightSystem.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreightSystem.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly FreightDbContext _dbContext;

    public DocumentsController(FreightDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("upload")]
    [Authorize(Policy = "OperationPolicy")]
    public async Task<IActionResult> Upload([FromForm] int shipmentId, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "يرجى إرسال ملف صالح" });

        var shipment = await _dbContext.Shipments.FindAsync(shipmentId);
        if (shipment == null)
            return NotFound(new { message = "الشحنة غير موجودة" });

        var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(uploadsRoot))
            Directory.CreateDirectory(uploadsRoot);

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadsRoot, uniqueFileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var document = new Document
        {
            ShipmentId = shipmentId,
            FileName = file.FileName,
            FilePath = filePath,
            Type = DocumentType.Other,
            UploadedAt = DateTime.UtcNow
        };

        await _dbContext.Documents.AddAsync(document);
        await _dbContext.SaveChangesAsync();

        return Ok(document);
    }

    [HttpGet("shipment/{shipmentId:int}")]
    [Authorize(Policy = "SalesPolicy")]
    public async Task<IActionResult> GetByShipment(int shipmentId)
    {
        var docs = _dbContext.Documents.Where(d => d.ShipmentId == shipmentId).ToList();
        return Ok(docs);
    }
}

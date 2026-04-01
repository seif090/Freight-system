using FreightSystem.Api.Filters;
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
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly FreightDbContext _dbContext;

    public AdminController(IUserRepository userRepository, FreightDbContext dbContext)
    {
        _userRepository = userRepository;
        _dbContext = dbContext;
    }

    [HttpGet("roles")]
    [XDescription("Fetch all roles for RBAC.", "جلب جميع الأدوار للنظام.")]
    public IActionResult GetRoles()
    {
        var roles = _dbContext.Roles.Select(r => new { r.Id, r.Name }).ToList();
        return Ok(roles);
    }

    [HttpPost("roles")]
    [XDescription("Create a new role.", "إنشاء دور جديد.")]
    public async Task<IActionResult> CreateRole([FromBody] Role role)
    {
        if (string.IsNullOrWhiteSpace(role.Name))
            return BadRequest(new { message = "Role name is required." });

        if (_dbContext.Roles.Any(r => r.Name == role.Name))
            return Conflict(new { message = "Role already exists." });

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRoles), new { id = role.Id }, role);
    }

    [HttpPost("users/{userId}/roles")]
    [XDescription("Assign a role to a user.", "تعيين دور إلى مستخدم.")]
    public async Task<IActionResult> AssignRole(int userId, [FromBody] string roleName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound();

        var role = _dbContext.Roles.SingleOrDefault(r => r.Name == roleName);
        if (role == null) return NotFound(new { message = "Role not found." });

        if (_dbContext.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == role.Id))
            return Conflict(new { message = "Role already assigned." });

        _dbContext.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id });
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}

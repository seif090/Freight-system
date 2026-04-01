using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FreightSystem.Api.Controllers;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, DateTime ExpiresAt);

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    private static readonly List<(string Username, string Password, string Role)> Users = new()
    {
        ("admin", "Admin123!", "Admin"),
        ("operation", "Op123!", "Operation"),
        ("accountant", "Ac123!", "Accountant"),
        ("sales", "Sales123!", "Sales")
    };

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = Users.SingleOrDefault(u => u.Username == request.Username && u.Password == request.Password);
        if (user == default)
            return Unauthorized(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة" });

        var jwtKey = _configuration.GetValue<string>("JwtSettings:Secret") ?? "default_super_secure_key_please_change";
        var jwtIssuer = _configuration.GetValue<string>("JwtSettings:Issuer") ?? "FreightSystem";
        var expiryMinutes = _configuration.GetValue<int>("JwtSettings:ExpiryMinutes");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("role", user.Role),
            new Claim("language", "ar")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtIssuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes > 0 ? expiryMinutes : 120),
            signingCredentials: creds);

        var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new LoginResponse(encodedToken, token.ValidTo));
    }
}

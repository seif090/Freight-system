using FreightSystem.Api.Filters;
using FreightSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
    private readonly IUserRepository _userRepository;

    public AuthController(IConfiguration configuration, IUserRepository userRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    [XDescription("Authenticate user and return JWT token.", "المصادقة على المستخدم وإرجاع رمز JWT.")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة" });

        if (!VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة" });

        var jwtKey = _configuration.GetValue<string>("JwtSettings:Secret") ?? "default_super_secure_key_please_change";
        var jwtIssuer = _configuration.GetValue<string>("JwtSettings:Issuer") ?? "FreightSystem";
        var expiryMinutes = _configuration.GetValue<int>("JwtSettings:ExpiryMinutes");

        var roles = await _userRepository.GetUserRolesAsync(user.Id);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("language", "ar")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role));
        }

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

    private static bool VerifyPassword(string plainPassword, string hash)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(plainPassword);
        var computedHash = Convert.ToBase64String(sha256.ComputeHash(bytes));
        return computedHash == hash;
    }
}

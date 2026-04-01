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
public record LoginResponse(string Token, string RefreshToken, DateTime ExpiresAt);
public record RefreshRequest(string Token, string RefreshToken);

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly Dictionary<string, string> _refreshTokens = new();
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
        var refreshToken = GenerateRefreshToken();
        _refreshTokens[refreshToken] = user.Username;

        return Ok(new LoginResponse(encodedToken, refreshToken, token.ValidTo));
    }

    [HttpPost("refresh")]
    [XDescription("Refresh an existing JWT using refresh token.", "تحديث JWT عبر رمز التحديث.")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest();

        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
            return BadRequest(new { message = "Invalid token." });

        if (!_refreshTokens.TryGetValue(request.RefreshToken, out var username) || username != principal.Identity?.Name)
            return BadRequest(new { message = "Invalid refresh token." });

        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
            return Unauthorized();

        var roles = await _userRepository.GetUserRolesAsync(user.Id);

        var jwtKey = _configuration.GetValue<string>("JwtSettings:Secret") ?? "default_super_secure_key_please_change";
        var jwtIssuer = _configuration.GetValue<string>("JwtSettings:Issuer") ?? "FreightSystem";
        var expiryMinutes = _configuration.GetValue<int>("JwtSettings:ExpiryMinutes");

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

        var newToken = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtIssuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes > 0 ? expiryMinutes : 120),
            signingCredentials: creds);

        var encodedNewToken = new JwtSecurityTokenHandler().WriteToken(newToken);
        var newRefreshToken = GenerateRefreshToken();

        _refreshTokens.Remove(request.RefreshToken);
        _refreshTokens[newRefreshToken] = user.Username;

        return Ok(new LoginResponse(encodedNewToken, newRefreshToken, newToken.ValidTo));
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtKey = _configuration.GetValue<string>("JwtSettings:Secret") ?? "default_super_secure_key_please_change";
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration.GetValue<string>("JwtSettings:Issuer") ?? "FreightSystem",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            return null;

        return principal;
    }

    private static bool VerifyPassword(string plainPassword, string hash)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(plainPassword);
        var computedHash = Convert.ToBase64String(sha256.ComputeHash(bytes));
        return computedHash == hash;
    }
}

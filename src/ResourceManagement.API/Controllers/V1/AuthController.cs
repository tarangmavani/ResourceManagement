using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ResourceManagement.Application.DTOs;

namespace ResourceManagement.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>Generate a JWT token for a mock user.</summary>
    [HttpPost("token")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 401)]
    public IActionResult GenerateToken([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("Token generation requested for user: {Username}", request.Username);

        string? role = null;

        // Hardcoded mock user credentials check
        if (string.Equals(request.Username, "admin", StringComparison.OrdinalIgnoreCase) && request.Password == "Admin@123")
        {
            role = "Admin";
        }
        else if (string.Equals(request.Username, "user", StringComparison.OrdinalIgnoreCase) && request.Password == "User@123")
        {
            role = "User";
        }

        if (role == null)
        {
            _logger.LogWarning("Authentication failed for user: {Username}", request.Username);
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail("Invalid username or password."));
        }

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT SecretKey not configured.");
        }

        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutesStr = jwtSettings["ExpiryMinutes"] ?? "60";
        if (!double.TryParse(expiryMinutesStr, out var expiryMinutes))
        {
            expiryMinutes = 60;
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var response = new LoginResponseDto
        {
            Token = tokenString,
            Username = request.Username,
            Role = role,
            Expiration = expires
        };

        _logger.LogInformation("Token generated successfully for user: {Username} with role: {Role}", request.Username, role);
        return Ok(ApiResponse<LoginResponseDto>.Ok(response, "Token generated successfully."));
    }
}

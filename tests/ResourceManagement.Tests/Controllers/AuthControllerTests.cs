using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ResourceManagement.API.Controllers.V1;
using ResourceManagement.Application.DTOs;
using Xunit;

namespace ResourceManagement.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IConfigurationSection> _jwtSectionMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _configMock = new Mock<IConfiguration>();
        _jwtSectionMock = new Mock<IConfigurationSection>();
        _loggerMock = new Mock<ILogger<AuthController>>();

        _jwtSectionMock.Setup(s => s["SecretKey"]).Returns("ResourceManagement_SuperSecretKey_2024_MinLength32Chars!");
        _jwtSectionMock.Setup(s => s["Issuer"]).Returns("ResourceManagement.API");
        _jwtSectionMock.Setup(s => s["Audience"]).Returns("ResourceManagement.Client");
        _jwtSectionMock.Setup(s => s["ExpiryMinutes"]).Returns("60");

        _configMock.Setup(c => c.GetSection("JwtSettings")).Returns(_jwtSectionMock.Object);

        _controller = new AuthController(_configMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void GenerateToken_WithValidAdminCredentials_ReturnsOk_WithToken()
    {
        var request = new LoginRequestDto { Username = "admin", Password = "Admin@123" };

        var result = _controller.GenerateToken(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoginResponseDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("admin", response.Data.Username);
        Assert.Equal("Admin", response.Data.Role);
        Assert.False(string.IsNullOrEmpty(response.Data.Token));
    }

    [Fact]
    public void GenerateToken_WithValidUserCredentials_ReturnsOk_WithToken()
    {
        var request = new LoginRequestDto { Username = "user", Password = "User@123" };

        var result = _controller.GenerateToken(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoginResponseDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("user", response.Data.Username);
        Assert.Equal("User", response.Data.Role);
        Assert.False(string.IsNullOrEmpty(response.Data.Token));
    }

    [Fact]
    public void GenerateToken_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var request = new LoginRequestDto { Username = "admin", Password = "wrongpassword" };

        var result = _controller.GenerateToken(request);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoginResponseDto>>(unauthorizedResult.Value);
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Invalid username or password.", response.Message);
    }
}

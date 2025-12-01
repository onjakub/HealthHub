using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McpHealtHubServer.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace McpHealtHubServer.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly Mock<ILogger<JwtTokenService>> _loggerMock;
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        _loggerMock = new Mock<ILogger<JwtTokenService>>();
        var validToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjI1MTYyMzkwMjJ9.test";
        _service = new JwtTokenService(validToken, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_WithNullToken_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JwtTokenService(null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjI1MTYyMzkwMjJ9.test";
        Assert.Throws<ArgumentNullException>(() => new JwtTokenService(token, null!));
    }

    [Fact]
    public void IsTokenValid_WithValidToken_ReturnsTrue()
    {
        // Act
        var result = _service.IsTokenValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetToken_ReturnsOriginalToken()
    {
        // Act
        var result = _service.GetToken();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", result);
    }

    [Fact]
    public void GetTokenExpiration_ReturnsValidDate()
    {
        // Act
        var result = _service.GetTokenExpiration();

        // Assert
        Assert.NotEqual(DateTime.MinValue, result);
        Assert.True(result > DateTime.UtcNow);
    }

    [Fact]
    public void IsTokenExpiringSoon_WithFarFutureToken_ReturnsFalse()
    {
        // Act
        var result = _service.IsTokenExpiringSoon(5);

        // Assert
        Assert.False(result);
    }
}
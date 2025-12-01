using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McpHealtHubServer.Exceptions;
using McpHealtHubServer.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace McpHealtHubServer.Tests.Services;

public class InputSanitizerTests
{
    private readonly Mock<ILogger<InputSanitizer>> _loggerMock;
    private readonly InputSanitizer _sanitizer;

    public InputSanitizerTests()
    {
        _loggerMock = new Mock<ILogger<InputSanitizer>>();
        _sanitizer = new InputSanitizer(_loggerMock.Object);
    }

    [Fact]
    public void SanitizeSearchTerm_WithNullInput_ReturnsEmptyString()
    {
        // Act
        var result = _sanitizer.SanitizeSearchTerm(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SanitizeSearchTerm_WithEmptyInput_ReturnsEmptyString()
    {
        // Act
        var result = _sanitizer.SanitizeSearchTerm(string.Empty);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SanitizeSearchTerm_WithWhitespaceInput_ReturnsEmptyString()
    {
        // Act
        var result = _sanitizer.SanitizeSearchTerm("   ");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SanitizeSearchTerm_WithValidInput_ReturnsSanitizedString()
    {
        // Arrange
        var input = "John Doe";

        // Act
        var result = _sanitizer.SanitizeSearchTerm(input);

        // Assert
        Assert.Equal("John Doe", result);
    }

    [Fact]
    public void SanitizeSearchTerm_WithSqlInjection_RemovesInjectionPatterns()
    {
        // Arrange
        var input = "John'; DROP TABLE Patients; --";

        // Act
        var result = _sanitizer.SanitizeSearchTerm(input);

        // Assert
        Assert.DoesNotContain("DROP TABLE", result);
        Assert.DoesNotContain("--", result);
        Assert.DoesNotContain(";", result);
    }

    [Fact]
    public void SanitizeSearchTerm_WithXssInjection_RemovesInjectionPatterns()
    {
        // Arrange
        var input = "John<script>alert('xss')</script>";

        // Act
        var result = _sanitizer.SanitizeSearchTerm(input);

        // Assert
        Assert.DoesNotContain("<script>", result);
        Assert.DoesNotContain("</script>", result);
    }

    [Fact]
    public void SanitizeSearchTerm_WithLongInput_TruncatesTo200Characters()
    {
        // Arrange
        var input = new string('a', 250);

        // Act
        var result = _sanitizer.SanitizeSearchTerm(input);

        // Assert
        Assert.Equal(200, result.Length);
    }

    [Fact]
    public void SanitizeGraphQLQuery_WithNullInput_ThrowsValidationException()
    {
        // Act & Assert
        Assert.Throws<HealthHubValidationException>(() => _sanitizer.SanitizeGraphQLQuery(null!));
    }

    [Fact]
    public void SanitizeGraphQLQuery_WithEmptyInput_ThrowsValidationException()
    {
        // Act & Assert
        Assert.Throws<HealthHubValidationException>(() => _sanitizer.SanitizeGraphQLQuery(string.Empty));
    }

    [Fact]
    public void SanitizeGraphQLQuery_WithValidQuery_ReturnsSanitizedQuery()
    {
        // Arrange
        var query = "query { getPatients { id name } }";

        // Act
        var result = _sanitizer.SanitizeGraphQLQuery(query);

        // Assert
        Assert.Equal(query, result);
    }

    [Fact]
    public void SanitizeGraphQLQuery_WithSqlInjection_RemovesInjectionPatterns()
    {
        // Arrange
        var query = "query { getPatients(search: \"'; DROP TABLE Patients; --\") { id name } }";

        // Act
        var result = _sanitizer.SanitizeGraphQLQuery(query);

        // Assert
        Assert.DoesNotContain("DROP TABLE", result);
        Assert.DoesNotContain("--", result);
    }

    [Fact]
    public void IsValidGuid_WithEmptyGuid_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.IsValidGuid(Guid.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidGuid_WithValidGuid_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = _sanitizer.IsValidGuid(guid);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidPagination_WithValidValues_ReturnsTrue()
    {
        // Act
        var result = _sanitizer.IsValidPagination(1, 10);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidPagination_WithInvalidPage_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.IsValidPagination(0, 10);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidPagination_WithInvalidPageSize_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.IsValidPagination(1, 0);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidPagination_WithLargePageSize_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.IsValidPagination(1, 1001);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidLimit_WithValidLimit_ReturnsTrue()
    {
        // Act
        var result = _sanitizer.IsValidLimit(100);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidLimit_WithInvalidLimit_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.IsValidLimit(0);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidLimit_WithLargeLimit_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.IsValidLimit(10001);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidAgeRange_WithValidRange_ReturnsTrue()
    {
        // Act
        var result = _sanitizer.IsValidAgeRange(18, 65);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidAgeRange_WithNegativeMinAge_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.IsValidAgeRange(-1, 65);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidAgeRange_WithNegativeMaxAge_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.IsValidAgeRange(18, -1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidAgeRange_WithMinGreaterThanMax_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.IsValidAgeRange(65, 18);

        // Assert
        Assert.False(result);
    }
}
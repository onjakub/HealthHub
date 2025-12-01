using System;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace McpHealtHubServer.Tools;

/// <summary>
/// Sample MCP tools for demonstration and testing purposes.
/// These tools can be invoked by MCP clients to perform simple operations
/// and verify that the MCP server is functioning correctly.
///
/// This tool is useful for:
/// - Testing MCP server connectivity
/// - Verifying tool invocation works correctly
/// - Demonstrating basic MCP functionality
/// - Debugging client-server communication
/// </summary>
internal class RandomNumberTools
{
    private readonly ILogger<RandomNumberTools> _logger;

    public RandomNumberTools(ILogger<RandomNumberTools> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [McpServerTool]
    [Description("Generates a random number between the specified minimum and maximum values. Useful for testing MCP server functionality.")]
    public int GetRandomNumber(
        [Description("Minimum value (inclusive)")] int min = 0,
        [Description("Maximum value (exclusive)")] int max = 100)
    {
        _logger.LogInformation("Generating random number between {Min} and {Max}", min, max);
        
        if (min >= max)
        {
            _logger.LogWarning("Invalid range: min ({Min}) must be less than max ({Max})", min, max);
            throw new ArgumentException("Minimum value must be less than maximum value");
        }

        var randomNumber = Random.Shared.Next(min, max);
        _logger.LogDebug("Generated random number: {RandomNumber}", randomNumber);
        
        return randomNumber;
    }
}
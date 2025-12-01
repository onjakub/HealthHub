using System.ComponentModel.DataAnnotations;

namespace McpHealtHubServer.Configuration;

public class HealthHubConfiguration
{
    public const string SectionName = "HealthHub";
    
    [Required]
    [Url]
    public string ApiUrl { get; set; } = "http://localhost:5000";
    
    [Required]
    public string JwtToken { get; set; } = string.Empty;
    
    [Range(1, 300)]
    public int HttpTimeoutSeconds { get; set; } = 30;
    
    [Range(1, 60)]
    public int CacheExpirationMinutes { get; set; } = 5;
}
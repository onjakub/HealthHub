namespace McpHealtHubServer.Exceptions;

public class HealthHubException : Exception
{
    public HealthHubException(string message) : base(message) { }
    
    public HealthHubException(string message, Exception innerException) : base(message, innerException) { }
}

public class HealthHubConfigurationException : HealthHubException
{
    public HealthHubConfigurationException(string message) : base(message) { }
    
    public HealthHubConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}

public class HealthHubApiException : HealthHubException
{
    public int StatusCode { get; }
    
    public HealthHubApiException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
    
    public HealthHubApiException(string message, int statusCode, Exception innerException) : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

public class HealthHubGraphQLException : HealthHubException
{
    public IReadOnlyList<string> GraphQLErrors { get; }
    
    public HealthHubGraphQLException(params string[] errors) : base($"GraphQL errors: {string.Join("; ", errors)}")
    {
        GraphQLErrors = errors;
    }
    
    public HealthHubGraphQLException(string message, Exception innerException) : base(message, innerException)
    {
        GraphQLErrors = new[] { message };
    }
}

public class HealthHubValidationException : HealthHubException
{
    public IReadOnlyList<string> ValidationErrors { get; }
    
    public HealthHubValidationException(IReadOnlyList<string> errors) : base($"Validation errors: {string.Join("; ", errors)}")
    {
        ValidationErrors = errors;
    }
    
    public HealthHubValidationException(string message) : base(message)
    {
        ValidationErrors = new List<string> { message };
    }
}
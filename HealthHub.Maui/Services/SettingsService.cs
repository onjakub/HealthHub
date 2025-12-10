namespace HealthHub.Maui.Services;

public class SettingsService : ISettingsService
{
    private const string GraphQLEndpointKey = "graphql_endpoint";
    private const string UseLocalStorageKey = "use_local_storage";

    public string? GraphQLEndpoint { get; set; } = "http://localhost:5000/graphql/";
    public bool UseLocalStorage { get; set; } = false;

    public async Task SaveSettingsAsync()
    {
        try
        {
            await SecureStorage.Default.SetAsync(GraphQLEndpointKey, GraphQLEndpoint ?? string.Empty);
            await SecureStorage.Default.SetAsync(UseLocalStorageKey, UseLocalStorage.ToString());
        }
        catch (Exception)
        {
            // SecureStorage not available or other error
        }
    }

    public async Task LoadSettingsAsync()
    {
        try
        {
            var endpoint = await SecureStorage.Default.GetAsync(GraphQLEndpointKey);
            if (!string.IsNullOrEmpty(endpoint))
            {
                GraphQLEndpoint = endpoint;
            }

            var useLocal = await SecureStorage.Default.GetAsync(UseLocalStorageKey);
            if (!string.IsNullOrEmpty(useLocal) && bool.TryParse(useLocal, out var useLocalValue))
            {
                UseLocalStorage = useLocalValue;
            }
        }
        catch (Exception)
        {
            // SecureStorage not available or other error
            // Use default values
        }
    }
}
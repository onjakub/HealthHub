namespace HealthHub.Maui.Services;

public class AuthService : IAuthService
{
    private string? _currentToken;
    public bool IsAuthenticated => !string.IsNullOrEmpty(_currentToken);
    public string? CurrentToken => _currentToken;

    public event EventHandler<bool>? AuthenticationStateChanged;

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            // For demo purposes, we'll accept any credentials
            // In a real app, this would make an API call to authenticate
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                _currentToken = $"demo-token-{Guid.NewGuid()}";
                AuthenticationStateChanged?.Invoke(this, true);
                await SaveTokenAsync();
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> LogoutAsync()
    {
        _currentToken = null;
        AuthenticationStateChanged?.Invoke(this, false);
        await ClearTokenAsync();
        return true;
    }

    private async Task SaveTokenAsync()
    {
        if (!string.IsNullOrEmpty(_currentToken))
        {
            await SecureStorage.Default.SetAsync("auth_token", _currentToken);
        }
    }

    private async Task ClearTokenAsync()
    {
        await SecureStorage.Default.SetAsync("auth_token", string.Empty);
    }

    public async Task LoadTokenAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token) && token != string.Empty)
            {
                _currentToken = token;
                AuthenticationStateChanged?.Invoke(this, true);
            }
        }
        catch (Exception)
        {
            // SecureStorage not available or other error
        }
    }
}
namespace HealthHub.Maui.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    Task<bool> LogoutAsync();
    bool IsAuthenticated { get; }
    string? CurrentToken { get; }
    event EventHandler<bool>? AuthenticationStateChanged;
}
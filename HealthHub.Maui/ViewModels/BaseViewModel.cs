using CommunityToolkit.Mvvm.ComponentModel;

namespace HealthHub.Maui.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    protected void SetError(string? errorMessage)
    {
        ErrorMessage = errorMessage;
        HasError = !string.IsNullOrEmpty(errorMessage);
    }

    protected void ClearError()
    {
        ErrorMessage = null;
        HasError = false;
    }

    protected async Task ExecuteWithLoading(Func<Task> action)
    {
        IsLoading = true;
        ClearError();
        
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            SetError($"An error occurred: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task<T> ExecuteWithLoading<T>(Func<Task<T>> action)
    {
        IsLoading = true;
        ClearError();
        
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            SetError($"An error occurred: {ex.Message}");
            return default!;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
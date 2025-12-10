using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HealthHub.Maui.Services;

namespace HealthHub.Maui.ViewModels;

public partial class AddPatientViewModel : BaseViewModel
{
    private readonly IPatientService _patientService;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private DateOnly _dateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-25));

    public AddPatientViewModel(IPatientService patientService)
    {
        _patientService = patientService;
        Title = "Add New Patient";
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            SetError("Please enter both first name and last name");
            return;
        }

        if (DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
        {
            SetError("Date of birth cannot be in the future");
            return;
        }

        await ExecuteWithLoading(async () =>
        {
            try
            {
                await _patientService.CreatePatientAsync(FirstName, LastName, DateOfBirth);
                await Shell.Current.GoToAsync("//PatientsPage");
            }
            catch (Exception ex)
            {
                SetError($"Failed to create patient: {ex.Message}");
            }
        });
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("//PatientsPage");
    }
}
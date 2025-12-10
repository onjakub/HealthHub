using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HealthHub.Maui.Models;
using HealthHub.Maui.Services;
using System.Collections.ObjectModel;

namespace HealthHub.Maui.ViewModels;

public partial class PatientDetailViewModel : BaseViewModel
{
    private readonly IPatientService _patientService;
    private readonly IDiagnosticResultService _diagnosticResultService;

    [ObservableProperty]
    private PatientDetailDto? _patient;

    [ObservableProperty]
    private ObservableCollection<DiagnosticResultDto> _diagnosticResults = new();

    [ObservableProperty]
    private string _newDiagnosis = string.Empty;

    [ObservableProperty]
    private string _newNotes = string.Empty;

    public PatientDetailViewModel(IPatientService patientService, IDiagnosticResultService diagnosticResultService)
    {
        _patientService = patientService;
        _diagnosticResultService = diagnosticResultService;
        Title = "Patient Details";
    }

    [RelayCommand]
    private async Task LoadPatient(Guid patientId)
    {
        await ExecuteWithLoading(async () =>
        {
            Patient = await _patientService.GetPatientAsync(patientId);
            
            if (Patient != null)
            {
                DiagnosticResults.Clear();
                foreach (var result in Patient.DiagnosticResults)
                {
                    DiagnosticResults.Add(result);
                }
                
                Title = $"{Patient.FirstName} {Patient.LastName}";
            }
        });
    }

    [RelayCommand]
    private async Task AddDiagnosticResult()
    {
        if (Patient == null || string.IsNullOrWhiteSpace(NewDiagnosis))
        {
            SetError("Please enter a diagnosis");
            return;
        }

        await ExecuteWithLoading(async () =>
        {
            try
            {
                var result = await _diagnosticResultService.AddDiagnosticResultAsync(
                    Patient.Id, NewDiagnosis, string.IsNullOrWhiteSpace(NewNotes) ? null : NewNotes);
                
                DiagnosticResults.Insert(0, result); // Add to top
                
                // Clear input fields
                NewDiagnosis = string.Empty;
                NewNotes = string.Empty;
                ClearError();
            }
            catch (Exception ex)
            {
                SetError($"Failed to add diagnosis: {ex.Message}");
            }
        });
    }

    [RelayCommand]
    private async Task Refresh()
    {
        if (Patient != null)
        {
            await LoadPatient(Patient.Id);
        }
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Application.Current?.MainPage?.Navigation.PopAsync();
    }

    public override Task OnAppearing()
    {
        // This would be called when the page appears
        return base.OnAppearing();
    }
}
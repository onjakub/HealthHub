using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HealthHub.Maui.Models;
using HealthHub.Maui.Services;
using System.Collections.ObjectModel;

namespace HealthHub.Maui.ViewModels;

public partial class PatientsViewModel : BaseViewModel
{
    private readonly IPatientService _patientService;

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private bool _hasNextPage;

    [ObservableProperty]
    private bool _hasPreviousPage;

    [ObservableProperty]
    private ObservableCollection<PatientDto> _patients = new();

    public PatientsViewModel(IPatientService patientService)
    {
        _patientService = patientService;
        Title = "Patients";
    }

    [RelayCommand]
    private async Task LoadPatients()
    {
        await ExecuteWithLoading(async () =>
        {
            var response = await _patientService.GetPatientsAsync(SearchTerm, CurrentPage);
            
            Patients.Clear();
            foreach (var patient in response.Nodes)
            {
                Patients.Add(patient);
            }

            TotalPages = (int)Math.Ceiling((double)response.TotalCount / 20); // Assuming pageSize of 20
            HasNextPage = response.PageInfo.HasNextPage;
            HasPreviousPage = response.PageInfo.HasPreviousPage;
        });
    }

    [RelayCommand]
    private async Task Search()
    {
        CurrentPage = 1;
        await LoadPatients();
    }

    [RelayCommand]
    private async Task NextPage()
    {
        if (HasNextPage)
        {
            CurrentPage++;
            await LoadPatients();
        }
    }

    [RelayCommand]
    private async Task PreviousPage()
    {
        if (HasPreviousPage)
        {
            CurrentPage--;
            await LoadPatients();
        }
    }

    [RelayCommand]
    private async Task AddPatient()
    {
        await Application.Current?.MainPage?.Navigation.PushAsync(
            Application.Current?.Handler?.MauiContext?.Services?.GetService<AddPatientPage>());
    }

    [RelayCommand]
    private async Task SelectPatient(PatientDto patient)
    {
        if (patient != null)
        {
            await Application.Current?.MainPage?.Navigation.PushAsync(
                Application.Current?.Handler?.MauiContext?.Services?.GetService<PatientDetailPage>());
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadPatients();
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();
        await LoadPatients();
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConferenceApp.Services;
using System.Diagnostics;

namespace ConferenceApp.ViewModels;

public partial class FlagsDebugViewModel : ObservableObject
{
    private readonly IFeatureFlagService _flagService;

    [ObservableProperty] private bool _sessionFeedbackEnabled;

    [ObservableProperty] private string _currentVersion = string.Empty;

    [ObservableProperty] private string _currentETag = string.Empty;

    public FlagsDebugViewModel(IFeatureFlagService flagService)
    {
        _flagService = flagService;
        _flagService.FlagsChanged += OnFlagsChanged;
        
        LoadCurrentFlags();
    }

    private void LoadCurrentFlags()
    {
        SessionFeedbackEnabled = _flagService.CurrentFlags.SessionFeedbackEnabled;
        CurrentVersion = _flagService.CurrentFlags.Version;
        CurrentETag = _flagService.CurrentFlags.ETag ?? "None";
    }

    private void OnFlagsChanged(object? sender, EventArgs e)
    {
        LoadCurrentFlags();
    }

    [RelayCommand]
    private async Task ToggleSessionFeedbackAsync()
    {
        var newValue = !SessionFeedbackEnabled;
        await _flagService.UpdateFlagAsync(nameof(_flagService.CurrentFlags.SessionFeedbackEnabled), newValue);
        Debug.WriteLine($"FlagsDebugViewModel: Toggled SessionFeedbackEnabled to {newValue}");
    }


    [RelayCommand]
    private async Task RefreshFromRemoteAsync()
    {
        await _flagService.RefreshFromRemoteAsync();
    }
}

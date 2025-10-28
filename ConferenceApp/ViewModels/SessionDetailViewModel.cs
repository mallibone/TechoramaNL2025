using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConferenceApp.Models;
using ConferenceApp.Services;
using System.Collections.ObjectModel;

namespace ConferenceApp.ViewModels;

public partial class SessionDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IConferenceRepository _repository;
    private readonly IFeatureFlagService _flagService;

    [ObservableProperty]
    private Session? session;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isFavorite;

    [ObservableProperty]
    private bool isFeedbackEnabled;

    public SessionDetailViewModel(IConferenceRepository repository, IFeatureFlagService flagService)
    {
        _repository = repository;
        _flagService = flagService;
        _flagService.FlagsChanged += OnFlagsChanged;
        
        UpdateFeedbackEnabled();
    }

    private void OnFlagsChanged(object? sender, EventArgs e)
    {
        UpdateFeedbackEnabled();
    }

    private void UpdateFeedbackEnabled()
    {
        IsFeedbackEnabled = _flagService.CurrentFlags.SessionFeedbackEnabled;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("SessionId", out var sessionId))
        {
            _ = LoadSessionAsync(sessionId.ToString()!);
        }
    }

    private async Task LoadSessionAsync(string sessionId)
    {
        IsLoading = true;
        try
        {
            Session = await _repository.GetSessionByIdAsync(sessionId);
            if (Session != null)
            {
                IsFavorite = Session.IsFavorite;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading session: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ToggleFavoriteAsync()
    {
        if (Session == null) return;

        await _repository.ToggleFavoriteAsync(Session.Id);
        IsFavorite = !IsFavorite;
        Session.IsFavorite = IsFavorite;
    }

    [RelayCommand]
    public async Task SpeakerSelectedAsync(Speaker speaker)
    {
        if (speaker == null) return;
        // Navigate to speaker detail page (to be implemented)
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task OpenFeedbackAsync()
    {
        if (Session == null) return;
        
        await Shell.Current.GoToAsync("feedback", new Dictionary<string, object>
        {
            { "SessionId", Session.Id },
            { "SessionTitle", Session.Title }
        });
    }
}

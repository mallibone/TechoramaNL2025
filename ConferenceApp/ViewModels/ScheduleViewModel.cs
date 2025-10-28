using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConferenceApp.Models;
using ConferenceApp.Services;
using System.Collections.ObjectModel;

namespace ConferenceApp.ViewModels;

public partial class ScheduleViewModel : ObservableObject
{
    private readonly IConferenceRepository _repository;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private int selectedDayIndex;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string? selectedTrackId;

    [ObservableProperty] private ObservableCollection<Day> days = new();
    [ObservableProperty] private ObservableCollection<Track> tracks = new();
    [ObservableProperty] private ObservableCollection<SessionGroup> groupedSessions = new();

    public ScheduleViewModel(IConferenceRepository repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            // Load days and tracks
            var days = await _repository.GetDaysAsync();
            Days.Clear();
            foreach (var day in days)
            {
                Days.Add(day);
            }

            var tracks = await _repository.GetTracksAsync();
            Tracks.Clear();
            Tracks.Add(new Track { Id = null!, Name = "All Tracks" }); // Filter option
            foreach (var track in tracks)
            {
                Tracks.Add(track);
            }

            // Load sessions for first day
            if (Days.Any())
            {
                SelectedDayIndex = 0;
                await LoadSessionsAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            await _repository.RefreshDataAsync();
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task SessionSelectedAsync(Session session)
    {
        if (session == null) return;
        await Shell.Current.GoToAsync($"session", new Dictionary<string, object>
        {
            { "SessionId", session.Id }
        });
    }

    [RelayCommand]
    public async Task ToggleFavoriteAsync(Session session)
    {
        if (session == null) return;

        await _repository.ToggleFavoriteAsync(session.Id);
        session.IsFavorite = !session.IsFavorite;
    }

    partial void OnSelectedDayIndexChanged(int value)
    {
        _ = LoadSessionsAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadSessionsAsync();
    }

    partial void OnSelectedTrackIdChanged(string? value)
    {
        _ = LoadSessionsAsync();
    }

    private async Task LoadSessionsAsync()
    {
        try
        {
            var sessions = await _repository.GetSessionsByDayAsync(SelectedDayIndex);

            // Apply filters
            var filtered = sessions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLowerInvariant();
                filtered = filtered.Where(s =>
                    s.Title.ToLowerInvariant().Contains(search) ||
                    s.Abstract.ToLowerInvariant().Contains(search) ||
                    s.Speakers.Any(sp => sp.FullName.ToLowerInvariant().Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(SelectedTrackId))
            {
                filtered = filtered.Where(s => s.TrackIds.Contains(SelectedTrackId));
            }

            // Group by time slot
            var grouped = filtered
                .GroupBy(s => s.StartUtc)
                .OrderBy(g => g.Key)
                .Select(g => new SessionGroup(g.Key, g.ToList()))
                .ToList();

            GroupedSessions.Clear();
            foreach (var group in grouped)
            {
                GroupedSessions.Add(group);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading sessions: {ex.Message}");
        }
    }
}

public class SessionGroup : List<Session>
{
    public DateTime StartTime { get; }
    public string TimeDisplay => StartTime.ToLocalTime().ToString("HH:mm");

    public SessionGroup(DateTime startTime, List<Session> sessions) : base(sessions)
    {
        StartTime = startTime;
    }
}

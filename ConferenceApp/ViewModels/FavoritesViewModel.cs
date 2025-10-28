using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConferenceApp.Models;
using ConferenceApp.Services;
using System.Collections.ObjectModel;

namespace ConferenceApp.ViewModels;

public partial class FavoritesViewModel : ObservableObject
{
    private readonly IConferenceRepository _repository;

    [ObservableProperty]
    private bool isLoading;

    public ObservableCollection<Session> FavoriteSessions { get; } = new();

    public FavoritesViewModel(IConferenceRepository repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    public async Task LoadFavoritesAsync()
    {
        IsLoading = true;
        try
        {
            var favorites = await _repository.GetFavoriteSessionsAsync();
            FavoriteSessions.Clear();
            foreach (var session in favorites)
            {
                FavoriteSessions.Add(session);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading favorites: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
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
    public async Task RemoveFavoriteAsync(Session session)
    {
        if (session == null) return;

        await _repository.ToggleFavoriteAsync(session.Id);
        FavoriteSessions.Remove(session);
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConferenceApp.Services;

namespace ConferenceApp.ViewModels;

public partial class FeedbackViewModel : ObservableObject, IQueryAttributable
{
    private readonly IFeedbackService _feedbackService;

    [ObservableProperty]
    private string _sessionId = string.Empty;

    [ObservableProperty]
    private string _sessionTitle = string.Empty;

    [ObservableProperty]
    private int _rating = 5;

    [ObservableProperty]
    private string _comments = string.Empty;

    [ObservableProperty]
    private bool _isSubmitted;

    public FeedbackViewModel(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("SessionId", out var sessionId))
        {
            SessionId = sessionId.ToString() ?? string.Empty;
        }

        if (query.TryGetValue("SessionTitle", out var title))
        {
            SessionTitle = title.ToString() ?? string.Empty;
        }

        // Check if feedback already exists
        var existingFeedback = _feedbackService.GetFeedback(SessionId);
        if (existingFeedback != null)
        {
            Rating = existingFeedback.Rating;
            Comments = existingFeedback.Comments ?? string.Empty;
            IsSubmitted = true;
        }
        else
        {
            IsSubmitted = false;
        }
    }

    [RelayCommand]
    private async Task SubmitFeedbackAsync()
    {
        _feedbackService.SubmitFeedback(SessionId, Rating, Comments);
        IsSubmitted = true;

        await Task.Delay(1000); // Show confirmation briefly
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

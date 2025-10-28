using System.Diagnostics;
using ConferenceApp.Models;

namespace ConferenceApp.Services;

public class FeedbackService : IFeedbackService
{
    private readonly Dictionary<string, SessionFeedback> _feedbackStore = new();

    public void SubmitFeedback(string sessionId, int rating, string? comments)
    {
        var feedback = new SessionFeedback
        {
            SessionId = sessionId,
            Rating = rating,
            Comments = comments,
            SubmittedAt = DateTime.UtcNow
        };

        _feedbackStore[sessionId] = feedback;
        Debug.WriteLine($"FeedbackService: Feedback submitted for session {sessionId} (Rating: {rating})");
    }

    public SessionFeedback? GetFeedback(string sessionId)
    {
        return _feedbackStore.TryGetValue(sessionId, out var feedback) ? feedback : null;
    }

    public IReadOnlyList<SessionFeedback> GetAllFeedback()
    {
        return _feedbackStore.Values.ToList();
    }

    public void ClearAllFeedback()
    {
        _feedbackStore.Clear();
        Debug.WriteLine("FeedbackService: All feedback cleared");
    }
}

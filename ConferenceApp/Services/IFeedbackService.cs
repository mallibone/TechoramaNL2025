using ConferenceApp.Models;

namespace ConferenceApp.Services;

public interface IFeedbackService
{
    void SubmitFeedback(string sessionId, int rating, string? comments);
    SessionFeedback? GetFeedback(string sessionId);
    IReadOnlyList<SessionFeedback> GetAllFeedback();
    void ClearAllFeedback();
}

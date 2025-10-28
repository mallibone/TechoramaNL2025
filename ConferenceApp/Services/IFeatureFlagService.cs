using ConferenceApp.Models;

namespace ConferenceApp.Services;

public interface IFeatureFlagService
{
    FeatureFlags CurrentFlags { get; }
    event EventHandler? FlagsChanged;
    
    Task InitializeAsync();
    Task RefreshFromRemoteAsync();
    Task UpdateFlagAsync(string flagName, bool value);
}

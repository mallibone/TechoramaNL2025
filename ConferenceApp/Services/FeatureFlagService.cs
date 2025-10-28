using ConferenceApp.Models;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace ConferenceApp.Services;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly HttpClient _httpClient;
    private readonly RemoteConfig _config;
    private readonly JsonSerializerOptions _jsonOptions;
    
    private const string CacheFileName = "featureflags_cache.json";
    private const string ETagKey = "flags_cached_etag";
    
    private FeatureFlags _currentFlags;

    public FeatureFlags CurrentFlags => _currentFlags;
    public event EventHandler? FlagsChanged;

    public FeatureFlagService(HttpClient httpClient, RemoteConfig config)
    {
        _httpClient = httpClient;
        _config = config;
        _currentFlags = FeatureFlags.CreateDefault();
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task InitializeAsync()
    {
        Debug.WriteLine("FeatureFlagService: Initializing...");
        
        // Try to load from cache first (subsequent launches)
        var cachedFlags = await LoadFromCacheAsync();
        if (cachedFlags != null)
        {
            _currentFlags = cachedFlags;
            Debug.WriteLine($"FeatureFlagService: Loaded from cache (version: {_currentFlags.Version})");
        }
        else
        {
            // First launch: load from embedded bootstrap
            var bootstrapFlags = await LoadFromBootstrapAsync();
            if (bootstrapFlags != null)
            {
                _currentFlags = bootstrapFlags;
                Debug.WriteLine($"FeatureFlagService: Loaded from bootstrap (version: {_currentFlags.Version})");
                
                // Save to cache for next launch
                await SaveToCacheAsync(_currentFlags);
            }
        }
        
        // Try to refresh from remote in the background
        _ = Task.Run(async () =>
        {
            try
            {
                await RefreshFromRemoteAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FeatureFlagService: Background refresh failed: {ex.Message}");
            }
        });
    }

    public async Task RefreshFromRemoteAsync()
    {
        if (!_config.Enabled)
        {
            Debug.WriteLine("FeatureFlagService: Remote fetch disabled");
            return;
        }

        var networkAccess = Connectivity.Current.NetworkAccess;
        if (networkAccess != NetworkAccess.Internet)
        {
            Debug.WriteLine("FeatureFlagService: No internet connection");
            return;
        }

        try
        {
            Debug.WriteLine($"FeatureFlagService: Fetching {_config.FlagsFullUrl}");
            
            var request = new HttpRequestMessage(HttpMethod.Get, _config.FlagsFullUrl);
            request.Headers.TryAddWithoutValidation("x-ms-version", "2023-11-03");
            
            // Add ETag for conditional request
            var cachedETag = Preferences.Get(ETagKey, null);
            if (!string.IsNullOrWhiteSpace(cachedETag))
            {
                request.Headers.TryAddWithoutValidation("If-None-Match", cachedETag);
                Debug.WriteLine($"FeatureFlagService: Using cached ETag: {cachedETag}");
            }

            var response = await _httpClient.SendAsync(request);

            // Handle 304 Not Modified
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                Debug.WriteLine("FeatureFlagService: Flags not modified (304)");
                return;
            }

            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var newFlags = JsonSerializer.Deserialize<FeatureFlags>(jsonContent, _jsonOptions);

            if (newFlags != null)
            {
                // Extract ETag from response
                string? newETag = null;
                if (response.Headers.ETag != null)
                {
                    newETag = response.Headers.ETag.Tag;
                }
                else if (response.Headers.TryGetValues("ETag", out var values))
                {
                    newETag = values.FirstOrDefault();
                }

                if (!string.IsNullOrEmpty(newETag))
                {
                    Preferences.Set(ETagKey, newETag);
                    Debug.WriteLine($"FeatureFlagService: Received ETag: {newETag}");
                }

                // Save to cache
                await SaveToCacheAsync(newFlags);

                // Update current flags
                _currentFlags = newFlags;
                
                Debug.WriteLine($"FeatureFlagService: Successfully fetched flags (version: {newFlags.Version})");
                
                // Notify listeners
                FlagsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FeatureFlagService: Error fetching remote flags: {ex.Message}");
        }
    }

    public async Task UpdateFlagAsync(string flagName, bool value)
    {
        // Directly update the flag value
        switch (flagName)
        {
            case nameof(FeatureFlags.SessionFeedbackEnabled):
                _currentFlags.SessionFeedbackEnabled = value;
                break;
            // Add future flags here
        }
        
        // Save updated flags to cache
        await SaveToCacheAsync(_currentFlags);
        
        Debug.WriteLine($"FeatureFlagService: Updated {flagName} = {value}");
        
        // Notify listeners
        FlagsChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task<FeatureFlags?> LoadFromBootstrapAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("featureflags.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<FeatureFlags>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FeatureFlagService: Error loading bootstrap: {ex.Message}");
            return null;
        }
    }

    private async Task<FeatureFlags?> LoadFromCacheAsync()
    {
        try
        {
            var cacheFile = Path.Combine(FileSystem.CacheDirectory, CacheFileName);
            if (!File.Exists(cacheFile))
                return null;

            var json = await File.ReadAllTextAsync(cacheFile);
            return JsonSerializer.Deserialize<FeatureFlags>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FeatureFlagService: Error loading cache: {ex.Message}");
            return null;
        }
    }

    private async Task SaveToCacheAsync(FeatureFlags flags)
    {
        try
        {
            var cacheFile = Path.Combine(FileSystem.CacheDirectory, CacheFileName);
            var json = JsonSerializer.Serialize(flags, _jsonOptions);
            await File.WriteAllTextAsync(cacheFile, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FeatureFlagService: Error saving cache: {ex.Message}");
        }
    }

}

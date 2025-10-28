using ConferenceApp.Models;
using System.Text.Json;

namespace ConferenceApp.Services;

public class LocalJsonStore : ILocalStore
{
    private const string CacheFileName = "conference_cache.json";
    private const string VersionKey = "cached_version";
    private const string ETagKey = "cached_etag";
    private readonly JsonSerializerOptions _jsonOptions;

    public LocalJsonStore()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<ConferenceData?> LoadConferenceDataAsync()
    {
        try
        {
            // First try cached file
            var cachedData = await LoadFromCacheAsync();
            if (cachedData is not null)
            {
                return cachedData;
            }

            // Fall back to bundled bootstrap JSON
            return await LoadFromBootstrapAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading conference data: {ex.Message}");
            return null;
        }
    }

    public async Task SaveConferenceDataAsync(ConferenceData data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var filePath = GetCacheFilePath();
            await File.WriteAllTextAsync(filePath, json);
            
            // Save version
            Preferences.Set(VersionKey, data.ContentVersion);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving conference data: {ex.Message}");
        }
    }

    public Task<string> GetCachedVersionAsync()
    {
        return Task.FromResult(Preferences.Get(VersionKey, string.Empty));
    }

    public Task<string> GetCachedETagAsync()
    {
        return Task.FromResult(Preferences.Get(ETagKey, string.Empty));
    }

    public Task SetCachedETagAsync(string etag)
    {
        Preferences.Set(ETagKey, etag);
        return Task.CompletedTask;
    }

    private async Task<ConferenceData?> LoadFromCacheAsync()
    {
        var filePath = GetCacheFilePath();
        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<ConferenceData>(json, _jsonOptions);
    }

    private async Task<ConferenceData?> LoadFromBootstrapAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("conference.json");
        if (stream == null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ConferenceData>(json, _jsonOptions);
    }

    private string GetCacheFilePath()
    {
        return Path.Combine(FileSystem.CacheDirectory, CacheFileName);
    }
}

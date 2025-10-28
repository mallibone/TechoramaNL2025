using ConferenceApp.Models;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace ConferenceApp.Services;

public class RemoteContentService : IRemoteContentService
{
    private readonly HttpClient _httpClient;
    private readonly RemoteConfig _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public RemoteContentService(HttpClient httpClient, RemoteConfig config)
    {
        _httpClient = httpClient;
        _config = config;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<RemoteContentResult> FetchConferenceDataAsync(string? cachedETag = null)
    {
        if (!_config.Enabled)
        {
            Debug.WriteLine("RemoteContentService: Remote fetch disabled");
            return new RemoteContentResult { Success = false, ErrorMessage = "Remote fetch disabled" };
        }

        // Check connectivity
        var networkAccess = Connectivity.Current.NetworkAccess;
        if (networkAccess != NetworkAccess.Internet)
        {
            Debug.WriteLine($"RemoteContentService: No internet connection (NetworkAccess: {networkAccess})");
            return new RemoteContentResult { Success = false, ErrorMessage = "No internet connection" };
        }

        // Try with retry logic
        var maxRetries = 3;
        var delays = new[] { 1000, 2000, 4000 }; // Exponential backoff in milliseconds

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                Debug.WriteLine($"RemoteContentService: Fetching {_config.ContentFullUrl} (attempt {attempt + 1}/{maxRetries})");
                
                var request = new HttpRequestMessage(HttpMethod.Get, _config.ContentFullUrl);
                
                // Add ETag header for conditional request
                if (!string.IsNullOrEmpty(cachedETag)) request.Headers.TryAddWithoutValidation("If-None-Match", cachedETag);

                var response = await _httpClient.SendAsync(request);

                // Handle 304 Not Modified
                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    Debug.WriteLine("RemoteContentService: Content not modified (304)");
                    return new RemoteContentResult
                    {
                        Success = true,
                        NotModified = true,
                        ETag = cachedETag
                    };
                }

                response.EnsureSuccessStatusCode();

                // Read response
                var jsonContent = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<ConferenceData>(jsonContent, _jsonOptions);

                // Extract ETag from response
                string? newETag = null;
                if (response.Headers.ETag != null)
                {
                    newETag = response.Headers.ETag.Tag;
                    Debug.WriteLine($"RemoteContentService: Received ETag: {newETag}");
                }

                Debug.WriteLine($"RemoteContentService: Successfully fetched data (version: {data?.ContentVersion})");
                
                return new RemoteContentResult
                {
                    Success = true,
                    Data = data,
                    ETag = newETag,
                    NotModified = false
                };
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"RemoteContentService: HTTP error on attempt {attempt + 1}: {ex.Message}");
                
                if (attempt < maxRetries - 1)
                {
                    await Task.Delay(delays[attempt]);
                }
                else
                {
                    return new RemoteContentResult
                    {
                        Success = false,
                        ErrorMessage = $"HTTP error after {maxRetries} attempts: {ex.Message}"
                    };
                }
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"RemoteContentService: Timeout on attempt {attempt + 1}: {ex.Message}");
                
                if (attempt < maxRetries - 1)
                {
                    await Task.Delay(delays[attempt]);
                }
                else
                {
                    return new RemoteContentResult
                    {
                        Success = false,
                        ErrorMessage = $"Request timeout after {maxRetries} attempts"
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RemoteContentService: Unexpected error: {ex.Message}");
                return new RemoteContentResult
                {
                    Success = false,
                    ErrorMessage = $"Unexpected error: {ex.Message}"
                };
            }
        }

        return new RemoteContentResult
        {
            Success = false,
            ErrorMessage = "Failed after all retry attempts"
        };
    }
}

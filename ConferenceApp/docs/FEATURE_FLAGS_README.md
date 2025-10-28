# Feature Flags Implementation

This document describes the feature flag system and session feedback functionality.

## Overview

The app now supports dynamic feature flags that can be:
- Shipped with the app via `Resources/Raw/featureflags.json`
- Updated remotely from Azure Blob Storage
- Overridden locally in DEBUG builds via a debug UI

## Architecture

### Components

1. **FeatureFlags Model** (`Models/FeatureFlags.cs`)
   - Contains flag properties (e.g., `SessionFeedbackEnabled`)
   - Includes `Version` for tracking updates
   - Supports `ETag` for HTTP caching

2. **FeatureFlagService** (`Services/FeatureFlagService.cs`)
   - Singleton service managing flag state
   - Load order: Bootstrap → Cache → Remote → Overrides
   - Supports ETag-based conditional requests
   - Fires `FlagsChanged` event when flags update

3. **FeedbackService** (`Services/FeedbackService.cs`)
   - Singleton service storing session feedback in-memory
   - Persists feedback per session ID
   - For demo purposes only (data cleared on app restart)

### Feature: Session Feedback

When enabled via the `sessionFeedbackEnabled` flag:
- A "Give Feedback" button appears on session detail pages
- Users can rate sessions 1-5 stars and add comments
- Feedback is stored in-memory (demo only)

## Usage

### Enabling Session Feedback

#### Option 1: Update Bootstrap File
Edit `Resources/Raw/featureflags.json`:
```json
{
  "version": "1.0",
  "sessionFeedbackEnabled": true
}
```

#### Option 2: Update Remote Config
Upload a new `featureflags.json` to Azure Blob Storage at:
```
https://mauidatadrivenblob.blob.core.windows.net/main/featureflags.json
```

The app will automatically fetch updates on next launch.

#### Option 3: DEBUG Override (DEBUG builds only)
1. Launch the app in DEBUG mode
2. Navigate to the "Debug" tab
3. Open "Feature Flags"
4. Toggle "Session Feedback" switch
5. Override is stored locally and persists across app restarts

### Testing the Feature

1. **Enable the flag** using one of the methods above
2. Navigate to a session detail page (tap any session from Schedule or Favorites)
3. Verify the "💬 Give Feedback" button appears
4. Tap the button to open the feedback form
5. Rate the session (1-5 stars) and optionally add comments
6. Submit the feedback
7. Re-open the feedback form to verify it persists in memory

### DEBUG Features

The DEBUG-only "Feature Flags" page provides:
- Current flag values and version info
- Toggle switches to override flags locally
- "Refresh from Remote" to fetch latest flags from blob storage
- "Clear All Overrides" to reset to remote/default values

**Note:** The Debug tab is only visible in DEBUG builds and is automatically removed in Release builds.

## Remote Configuration

### Blob Storage Setup

The feature flags are fetched from the same Azure Blob Storage container as conference data:
- **Base URL**: `https://mauidatadrivenblob.blob.core.windows.net/main`
- **Flags Path**: `featureflags.json`

### ETag Support

The service uses HTTP ETag headers for efficient caching:
- Sends `If-None-Match` with cached ETag
- Handles `304 Not Modified` responses
- Only downloads when content changes
- Separate ETag from conference data (stored as `flags_cached_etag`)

### Cache Files

- **Bootstrap**: `Resources/Raw/featureflags.json` (shipped with app)
- **Cache**: `featureflags_cache.json` (in app cache directory)
- **Preferences Keys**:
  - `flags_cached_version`: Version of cached flags
  - `flags_cached_etag`: ETag for conditional requests
  - `flags_overrides_json`: DEBUG overrides (JSON dictionary)

## Adding New Flags

To add a new feature flag:

1. **Update the FeatureFlags model**:
```csharp
public class FeatureFlags
{
    // Existing flags
    public bool SessionFeedbackEnabled { get; set; }
    
    // Add your new flag
    public bool MyNewFeatureEnabled { get; set; }
}
```

2. **Update the bootstrap JSON**:
```json
{
  "version": "1.1",
  "sessionFeedbackEnabled": false,
  "myNewFeatureEnabled": false
}
```

3. **Apply overrides in FeatureFlagService.ApplyOverrides()**:
```csharp
private void ApplyOverrides()
{
    if (_overrides.TryGetValue(nameof(FeatureFlags.SessionFeedbackEnabled), out var sessionFeedbackOverride))
        _currentFlags.SessionFeedbackEnabled = sessionFeedbackOverride;
    
    // Add your flag override
    if (_overrides.TryGetValue(nameof(FeatureFlags.MyNewFeatureEnabled), out var myNewFeatureOverride))
        _currentFlags.MyNewFeatureEnabled = myNewFeatureOverride;
}
```

4. **Update FlagsDebugPage.xaml** to add a toggle for the new flag (DEBUG builds only)

5. **Use the flag in your ViewModels**:
```csharp
public MyViewModel(IFeatureFlagService flagService)
{
    _flagService = flagService;
    _flagService.FlagsChanged += OnFlagsChanged;
    UpdateFeatureEnabled();
}

private void UpdateFeatureEnabled()
{
    IsMyFeatureEnabled = _flagService.CurrentFlags.MyNewFeatureEnabled;
}
```

## Load Sequence

On app startup:
1. `App.xaml.cs` injects `IFeatureFlagService` and calls `InitializeAsync()`
2. Service loads DEBUG overrides from `Preferences`
3. Service attempts to load from cache (`featureflags_cache.json`)
4. If cache missing, loads from bootstrap (`Resources/Raw/featureflags.json`)
5. Applies overrides to current flags
6. Background task attempts remote fetch with ETag
7. On successful fetch, updates cache and fires `FlagsChanged` event
8. ViewModels subscribed to `FlagsChanged` update their UI-bound properties

## Notes

- Feature flags are **global** (not per-user or per-platform)
- Remote fetch happens in background and doesn't block startup
- Override precedence: **DEBUG overrides > Remote > Cached > Bootstrap**
- Version field is informational; ETag drives cache invalidation
- Feedback data is **in-memory only** and cleared on app restart (demo implementation)

## Future Enhancements

Potential improvements:
- Per-platform or per-environment flag variants
- A/B testing support with user cohorts
- Analytics integration to track flag usage
- Persistent feedback storage (local DB or API endpoint)
- Admin UI to manage flags without blob storage uploads

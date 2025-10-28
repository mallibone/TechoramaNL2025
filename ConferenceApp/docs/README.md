# Conference App - .NET MAUI

A cross-platform conference scheduling app built with .NET MAUI, featuring offline-first JSON data loading, session browsing, favorites management, and track filtering.

## Features Implemented ✅

- **Schedule Browsing**
  - View sessions by day with grouped time slots
  - Filter by track (Mobile, Cloud, Web, AI)
  - Search sessions by title, abstract, or speaker name
  - Pull-to-refresh for data updates

- **Session Details**
  - Full session information (title, abstract, level, time, room)
  - Speaker profiles with photos and social links
  - Track badges with custom colors
  - Workshop indicators
  - Add/remove favorites

- **Favorites Management**
  - Star sessions to add to "My Favorites"
  - Swipe to remove favorites
  - Persisted locally using Preferences

- **Data Architecture**
  - Offline-first with bootstrap JSON bundled in app
  - JSON file caching for updates
  - Repository pattern with dependency injection
  - MVVM with CommunityToolkit.Mvvm

- **UI/UX**
  - Tab-based navigation (Schedule, Favorites)
  - Day selector with friendly names
  - Track color-coded chips
  - Responsive layouts with CollectionView virtualization
  - SwipeView for quick actions

## Project Structure

```
ConferenceApp/
├── Models/               # Domain models (Session, Speaker, Track, etc.)
├── Services/             # Data layer (Repository, LocalStore)
├── ViewModels/           # MVVM ViewModels with CommunityToolkit
├── Views/                # XAML pages (Schedule, SessionDetail, Favorites)
├── Converters/           # Value converters for data binding
├── Resources/
│   └── Raw/
│       └── conference.json   # Bootstrap data
├── docs/
│   └── plan.md          # Architecture and planning doc
└── MauiProgram.cs       # DI configuration
```

## Sample Data

The app ships with a 2-day conference featuring:
- **11 sessions** across 2 days (Nov 15-16, 2025)
- **4 tracks**: Mobile Development, Cloud & Azure, Web Development, AI & Machine Learning
- **4 speakers** with full profiles
- **4 rooms** at Technopark Zurich

## Running the App

### macOS/Mac Catalyst
```bash
dotnet build -t:Run -f net9.0-maccatalyst
```

### iOS Simulator
```bash
dotnet build -t:Run -f net9.0-ios
```

### Android Emulator
```bash
dotnet build -t:Run -f net9.0-android
```

### Windows
```bash
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

## Dependencies

- **Microsoft.Maui.Controls** (9.0+)
- **CommunityToolkit.Mvvm** (8.4.0) - MVVM helpers, source generators

## Data Model

The `conference.json` schema includes:

- **Root**: schemaVersion, contentVersion, conference info, API endpoints
- **Days**: index, date, timezone (IANA), friendly name
- **Tracks**: id, name, colorHex, description, order
- **Rooms**: id, name, building, floor, capacity, coordinates
- **Speakers**: id, name, bio, company, job title, socials, headshot
- **Sessions**: id, title, abstract, level, tags, startUtc, endUtc, dayIndex, trackIds, speakerIds, roomId, capacity, flags (isLiveStream, isWorkshop, requiresRegistration)
- **Sponsors**: id, name, tier, logo, website, blurb

## Next Steps (Future Enhancements)

Based on the [plan.md](docs/plan.md):

1. **Remote Content Delivery**
   - Fetch updates from Azure Static Website + CDN
   - ETag/If-None-Match support for efficient updates
   - SHA-256 signature verification
   - Retry/backoff with Polly

2. **Enhanced Features**
   - Conflict detection for overlapping favorites
   - Calendar export (ICS generation)
   - Maps integration for venue/room navigation
   - Speaker detail page with sessions list
   - Sponsors page with tiers

3. **Platform Improvements**
   - Deep linking/App Links (session://{id})
   - Share session details
   - Localization (.resx for UI strings)
   - Dark/light theme refinements
   - Accessibility improvements (semantic properties, VoiceOver/TalkBack)

4. **Performance**
   - Compiled bindings optimization (enable MauiEnableXamlCBindingWithSourceCompilation)
   - Image prefetching for speakers/tracks
   - AOT compilation for Release builds

5. **Testing & CI/CD**
   - Unit tests for repository, mappers, time/timezone logic
   - UI tests with Appium
   - GitHub Actions for build/release
   - Azure Static Website deployment for JSON

## Architecture Decisions

- ✅ **Offline store**: JSON file cache only (no SQLite)
- ✅ **Hosting/CDN**: Azure Static Website + Azure CDN
- ✅ **Notifications**: Not included (users rely on calendar export)

See [docs/plan.md](docs/plan.md) for comprehensive architecture documentation.

## License

MIT License - Conference App Demo

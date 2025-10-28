# Conference App - Quick Start Guide

## 🚀 Running the App

Choose your platform:

```bash
# macOS (recommended for quick testing)
cd ConferenceApp
dotnet build -t:Run -f net9.0-maccatalyst

# iOS Simulator
dotnet build -t:Run -f net9.0-ios

# Android Emulator
dotnet build -t:Run -f net9.0-android

# Windows
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

## 📱 App Navigation

```
┌─────────────────────────────────────┐
│         Conference App              │
├─────────────────────────────────────┤
│  [Schedule Tab]  [Favorites Tab]    │
├─────────────────────────────────────┤
│                                     │
│  Schedule Tab:                      │
│  ┌───────────────────────────────┐  │
│  │ Day 1 | Day 2                 │  │ ← Day selector
│  ├───────────────────────────────┤  │
│  │ [Search...] [Track Filter ▼]  │  │ ← Filters
│  ├───────────────────────────────┤  │
│  │ 08:00                         │  │ ← Time slot
│  │  📱 Building Modern Mobile... │  │ ← Session
│  │  👤 Sarah Johnson             │  │
│  │  📍 Main Hall ⏱ 08:00-09:00  │  │
│  │  [Mobile Dev]                 │  │ ← Track chip
│  ├───────────────────────────────┤  │
│  │ 09:30                         │  │
│  │  ☁️ Azure Serverless...       │  │
│  │  ...                          │  │
│  └───────────────────────────────┘  │
│                                     │
│  Swipe right on session → Favorite │
│  Tap session → Session Detail       │
│                                     │
└─────────────────────────────────────┘

Session Detail:
┌─────────────────────────────────────┐
│  Building Modern Mobile Apps...     │
│  [⭐ Add to Favorites]              │
│                                     │
│  📅 Friday, November 15             │
│  ⏰ 08:00 - 09:00                   │
│  📍 Main Hall                       │
│                                     │
│  Abstract:                          │
│  Learn how to build...              │
│                                     │
│  Level: [Intermediate]              │
│  Tracks: [Mobile Development]       │
│                                     │
│  Speakers:                          │
│  ┌─────────────────────────────┐   │
│  │ [Photo] Sarah Johnson        │   │
│  │         Principal Architect  │   │
│  │         Mobile Innovators    │   │
│  └─────────────────────────────┘   │
└─────────────────────────────────────┘

Favorites Tab:
┌─────────────────────────────────────┐
│  My Favorite Sessions               │
│  ┌─────────────────────────────┐   │
│  │ ⭐ Building Modern Mobile... │   │ ← Swipe to remove
│  │  👤 Sarah Johnson           │   │
│  │  📅 Nov 15  ⏱ 08:00        │   │
│  │  [Mobile Dev]               │   │
│  ├─────────────────────────────┤   │
│  │ ⭐ Azure Serverless...      │   │
│  │  ...                        │   │
│  └─────────────────────────────┘   │
└─────────────────────────────────────┘
```

## 🎯 Key Features to Try

1. **Browse Schedule**
   - Switch between Day 1 and Day 2
   - Use search bar to find "Blazor" or "Mobile"
   - Filter by track dropdown

2. **Manage Favorites**
   - Swipe right on any session → "Favorite"
   - Or tap session → detail page → "Add to Favorites"
   - View all in Favorites tab
   - Swipe left to remove

3. **Session Details**
   - Tap any session to see full details
   - View speaker profiles
   - See room, time, level, tracks
   - Workshop sessions show special badge

4. **Pull to Refresh**
   - Pull down on schedule to refresh data

## 📊 Sample Data Included

- **Conference**: .NET Developer Conference 2025
- **Location**: Technopark Zurich, Switzerland
- **Dates**: November 15-16, 2025
- **Sessions**: 11 sessions
- **Tracks**: Mobile, Cloud, Web, AI
- **Speakers**: 4 speakers with profiles

## 🔧 Updating Conference Data

Edit `Resources/Raw/conference.json` to customize:

```json
{
  "conference": {
    "name": "Your Conference Name",
    "tz": "America/New_York",
    "startDate": "2025-12-01",
    "endDate": "2025-12-02"
  },
  "days": [...],
  "tracks": [...],
  "speakers": [...],
  "sessions": [...]
}
```

After editing, rebuild the app.

## 📚 Documentation

- `README.md` - Full project overview
- `docs/plan.md` - Architecture and planning document
- `IMPLEMENTATION.md` - Implementation summary

## 🐛 Build Notes

The app builds successfully with 6 warnings about XAML binding compilation optimizations. These are performance hints, not errors. The app runs correctly as-is.

To enable optimizations (optional):
Add to `ConferenceApp.csproj`:
```xml
<MauiEnableXamlCBindingWithSourceCompilation>true</MauiEnableXamlCBindingWithSourceCompilation>
```

## 💡 Tips

- Favorites persist across app restarts (stored in device Preferences)
- Times automatically convert from UTC to local timezone
- Search works on session title, abstract, and speaker names
- Track colors are customizable in JSON (colorHex field)

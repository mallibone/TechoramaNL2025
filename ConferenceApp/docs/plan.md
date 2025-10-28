# Conference App Plan (JSON‑driven .NET MAUI)

Last updated: 2025-10-23
Scope: 2–3 day conference with multiple tracks, sessions, speakers, and rooms. Offline‑first, CDN‑served JSON content, MVVM + Shell, cross‑platform (iOS/Android/Windows/macOS Catalyst).

---

## 1) Data model and JSON schema

Entities
- Session: id, title, abstract, level, tags[], startUtc, endUtc, dayIndex, trackIds[], speakerIds[], roomId, capacity, isLiveStream, recordingUrl, liveUrl, isWorkshop, requiresRegistration, imageUrls[], lastUpdated, version
- Speaker: id, fullName, bio, company, jobTitle, socials {twitter, mastodon, github, linkedin, website}, headshotUrl, tags[], lastUpdated
- Track: id, name, colorHex, description, order, iconUrl, lastUpdated
- Room: id, name, building, floor, capacity, mapCoordinates {lat, lon, level?}, indoorMapImageUrl, lastUpdated
- Day: index (0‑based), date (YYYY‑MM‑DD), tz (IANA, e.g., "Europe/Paris"), friendlyName
- Timeslot (optional helper): {startUtc, endUtc, sessionIds[]} — derivable from sessions
- Sponsor: id, name, tier, logoUrl, website, blurb
- Root meta: schemaVersion (SemVer), contentVersion (SemVer or increment), generatedAtUtc, conference {name, tz, venue, address, startDate, endDate}, api {baseUrl, assetsBaseUrl}, minAppVersion

Relationships
- M:N sessions ↔ speakers via speakerIds[]
- M:N sessions ↔ tracks via trackIds[]
- 1:N room ↔ sessions via roomId
- 1:N day ↔ sessions via dayIndex/date

Required fields
- ids, titles, startUtc, endUtc, abstract, level, tags[], roomId, capacity (>= 0), dayIndex, trackIds[], track colorHex (#RRGGBB/#AARRGGBB), lastUpdated, schemaVersion, contentVersion, conference.tz

i18n strategy
- Localized fields as objects: title: {"en":"…","fr":"…"} with default language fallback
- App UI strings via .resx; data localization resolved by CurrentUICulture with graceful fallback

Images/media
- Absolute HTTPS URLs; multiple densities or widths; prefer WebP/AVIF; cache‑bust with contentVersion query

Versioning & compatibility
- schemaVersion (SemVer) governs shape; contentVersion increments for content changes; minAppVersion gates breaking schema
- Prefer additive changes; deprecate via "deprecated": true; keep fields 1–2 releases after deprecation

Size limits & splitting
- Target < 1–2 MB compressed total (hard cap 5 MB)
- Shard: index.json (metadata + child URLs + ETags), days/day-1.json, days/day-2.json, speakers.json, tracks.json, rooms.json, sponsors.json
- Pagination generally unnecessary at this scale

Caching/HTTP
- Use strong ETag/If-None-Match and Last-Modified/If-Modified-Since; handle 304s
- Cache-Control: max-age=300 (device), longer s-maxage at CDN

Validation checklist (CI)
- Unique ids; referential integrity (speakerIds/trackIds/roomId exist)
- Time order (startUtc < endUtc); room double-booking checks; sessions map to valid dayIndex; consistent IANA tz
- Valid color hex; capacity >= 0; tags normalized; URLs https and reachable
- No orphan speakers/tracks; contentVersion monotonic; schemaVersion SemVer
- Optional: publish JSON Schema and validate in CI

---

## 2) Content delivery

Hosting
- Azure Blob Static Website + Azure CDN

Update flow
- Ship bootstrap JSON under app `Resources/Raw/`
- On startup/resume: fetch index.json with If-None-Match; compare contentVersion/ETag; download only changed shards; verify hashes/signatures; atomic swap; keep last‑good for rollback

Offline‑first
- JSON file cache + in‑memory indexes (sufficient for 2–3 day scale)
- Client-side filtering/search using LINQ over deserialized models
- Persist favorites locally using Preferences/SecureStorage (simple id list)

Reliability
- Retry/backoff with Polly (exponential + jitter), reasonable timeouts, circuit breaker; graceful fallback to cached content

Hardening
- Include SHA‑256 for each shard in index.json and an optional detached signature (e.g., Ed25519); embed public key in app; verify before accepting updates
- Enforce HTTPS; validate size and Content‑Type

Images
- MAUI UriImageSource with caching; tune CacheValidity; prefetch critical speaker/track images after first render

Cadence
- Publish content updates daily/as‑needed; app checks on cold start and every 2–4h while active (don’t rely on background fetch)

---

## 3) App architecture

Pattern
- MVVM with .NET Community Toolkit (trim‑friendly, mature). MVU/Comet remains niche

DI & composition
- Register services/repositories in `MauiProgram.cs` via Microsoft.Extensions.DependencyInjection; configure logging

Navigation
- MAUI Shell with routes for details and deep links; prefer IQueryAttributable over QueryProperty for trim/AOT safety

State management
- AppState service holds schedule snapshot + filters + favorites; ViewModels (ObservableObject/ObservableProperty) subscribe; use immutable filtered snapshots

Data layer
- IRemoteContent (HTTP + ETag + signature + Polly)
- ILocalStore (file cache + optional SQLite)
- IConferenceRepository (merge shards → domain models)
- DTO ↔ domain mappers; pure functions for conflict detection/filters

Testing
- Unit: repositories, mappers, time/tz logic, validators
- UI: Appium 2 cross‑platform smoke + navigation; snapshot tests for list rendering

CI/CD
- GitHub Actions: build Android (.aab) and iOS (.ipa), cache NuGet, sign artifacts; Fastlane for iOS/TestFlight and Play internal track
- Content pipeline CI validates schema, links, sizes, ETags, signatures; publish to CDN on main branch

---

## 4) UX features

Schedule browsing
- By day and by track; sticky headers; track color accents; room/capacity chips

Filters & search
- Filter by track, level, room, workshop/livestream; SearchBar across title/abstract/speaker (diacritics‑insensitive)

Favorites / My agenda
- Favorite toggle; My Agenda section; local calendar export optional; persisted locally; conflict warnings

Conflict detection
- Detect overlaps among favorites in time; suggest alternatives

Time zones
- Default to conference tz; toggle to device tz; display tz hint; store times in UTC; format with tz abbreviation

Now & Next
- "Happening now" and "Next up" by track/room; quick jump to current day/time

Calendar export
- Generate ICS for session(s); share sheet to open in calendar apps; batch export My Agenda

Maps/locations
- Venue map with pins; deep link to native Maps for outdoor; indoor static maps by floor with tappable POIs

Speaker profiles
- Bio, sessions list, social links, headshot; share speaker page

Sponsors, CoC, feedback
- Sponsor tiers/logos; Code of Conduct page; session feedback (deeplink to external form) with optional offline queue

---

## 5) Platform specifics

Background refresh
- Refresh on app open/resume; don't rely on background fetch (iOS constraints)

Deep linking/App Links
- Shell URI routes; Android intent filters + Digital Asset Links; iOS Universal Links + Associated Domains; custom app scheme fallback

Share sheets
- Use Essentials Share for ICS/links/sessions

Theming
- Light/dark; track colors in ResourceDictionary; ensure WCAG AA contrast; dynamic accents per track

Accessibility
- Semantic properties and accessible names; 44×44pt touch targets; high contrast; keyboard navigation on desktop; VoiceOver/TalkBack labels

Localization
- .resx for UI; platform configs (Info.plist CFBundleLocalizations, Windows Resources.resw); RTL readiness; follow OS language

Performance
- CollectionView virtualization; Recycle templating; incremental loading; avoid nested layouts; fixed image sizes; compiled bindings; avoid FromStream cache issues

Startup time
- Lazy‑load content post first frame; defer network; Release AOT + trimming; reduce DI graph; minimize constructor work

---

## 6) Data privacy & compliance

Telemetry/crash
- Sentry for MAUI (managed + native), or Firebase Crashlytics; App Center status to be monitored; prefer Sentry/Crashlytics in 2025

Privacy disclosures
- Apple privacy nutrition labels; retention policy; opt‑in analytics and “Disable analytics” toggle

Offline mode
- All critical features usable offline; show last‑updated; manual refresh affordance

---

## 7) Release & operations

Signing
- Android keystore (.aab); iOS certs/profiles; automate with Fastlane

Store listing
- Highlight offline capability, privacy, and permission rationale

Environments
- Staging vs production endpoints; build‑time define or remote toggle in index.json

Feature flags
- Remote flags in index.json; local overrides for QA; kill‑switch for experimental features

SLIs and error budgets
- Content fetch success rate, time‑to‑first‑schedule, crash‑free sessions; acceptable thresholds defined per release

Fallback content
- Always ship bootstrap schedule + last‑good cache; render minimal UI on failure

---

## 8) Risks & mitigations

- Time zone drift/DST: store UTC; compute with conference IANA tz; re‑evaluate on tz change; display tz
- Clock skew: prefer server timestamps; avoid relying purely on device time for critical logic
- Over‑fetching: shard JSON; leverage 304s; separate speakers/tracks/days
- Image bloat: WebP/AVIF; multiple sizes; CDN resizing; tune cache validity; lazy load
- Schema changes: SemVer; additive first; minAppVersion gate; dual‑parse for one release
- CDN cache issues: surrogate keys; purge on contentVersion; strong ETags; monitor 304/200 mix
- Network flakiness: retries with jitter; timeouts + circuit breaker; offline fallback; clear error UI
- iOS background constraints: refresh on open/resume; schedule notifications ahead; avoid background fetch reliance

---

## MAUI component mapping

- CollectionView: schedule lists, speaker grids, favorites (grouping by day/track)
- Shell: route‑based navigation, deep links, query parameters
- SearchBar + CollectionView: instant filtering/search
- RefreshView: pull‑to‑refresh content update
- SwipeView: quick favorite toggle or contextual actions
- CommunityToolkit.Mvvm: ObservableObject, RelayCommand, DI‑friendly ViewModels
- Essentials APIs: Preferences/SecureStorage, Connectivity, Share, Launcher/Browser, Maps interop
- Maps: venue/outdoor directions (MAUI Maps)
- Image/UriImageSource: remote images with caching (CachingEnabled, CacheValidity)

---

## Implementation steps (initial)

1) Add `docs/` (this file) and bootstrap JSON under `ConferenceApp/Resources/Raw/`
2) Register services in `MauiProgram.cs`: RemoteContent (HTTP+ETag+Polly), LocalStore (JSON file cache), ConferenceRepository, AppState
3) Define domain models and DTOs; mappers; validators (unit‑tested)
4) Configure Shell routes in `AppShell.xaml` (Schedule, SessionDetail, Speaker, Favorites, Sponsors, Settings)
5) Build Schedule view with day/track tabs, filters, search, and favorites
6) Implement ICS export and basic maps deep links
7) Wire CI for JSON validation and app builds; configure Azure Static Website + CDN for content delivery

---

## Architecture decisions

✅ **Offline store**: JSON file cache only (no SQLite) — sufficient for 2–3 day conference scale; client-side LINQ filtering  
✅ **Hosting/CDN**: Azure Static Web Apps (West Europe region)  
✅ **Notifications**: Not included — users rely on calendar export and manual agenda review

---

## Remote Content Updates Plan

**Status**: Ready to implement (2025-10-23)

### Configuration
- **Azure Region**: West Europe
- **Resource Group**: `rg-conferenceapp`
- **Static Web App**: `confapp-static-prod`
- **Environment**: Production only (no staging)
- **Access**: Public read, no authentication
- **Endpoint**: `https://confapp-static-prod.azurestaticapps.net/conference.json`

### Implementation Phases

#### Phase 1: Remote Content Service (Code)
- [ ] Create `Models/RemoteConfig.cs` - Configuration for remote URL
- [ ] Create `Services/IRemoteContentService.cs` - Interface
- [ ] Create `Services/RemoteContentService.cs` - HTTP client with:
  - ETag support (If-None-Match header, handle 304)
  - Retry logic: 3 attempts, exponential backoff (1s, 2s, 4s)
  - Connectivity check via `Connectivity.Current.NetworkAccess`
  - 10s timeout
- [ ] Update `Services/ILocalStore.cs` - Add ETag persistence methods
- [ ] Update `Services/LocalJsonStore.cs` - Implement ETag get/set via Preferences
- [ ] Update `Services/ConferenceRepository.cs` - Wire remote fetch in `RefreshDataAsync()`
- [ ] Update `MauiProgram.cs` - Register HttpClient and RemoteContentService

#### Phase 2: Azure Infrastructure Setup
- [ ] Create Azure CLI script for:
  - Resource group creation
  - Static Web App provisioning
  - Initial conference.json upload
- [ ] Configure CORS and cache headers in `staticwebapp.config.json`
- [ ] Verify public access and test endpoint

#### Phase 3: CI/CD (Optional)
- [ ] Create `.github/workflows/deploy-content.yml`
- [ ] JSON validation script
- [ ] Auto-deploy on push to main branch

#### Phase 4: UI Enhancements (Optional)
- [ ] Add "Last updated" timestamp in Schedule page
- [ ] Connection status indicator
- [ ] Success/error feedback toast

### Offline-First Flow
```
App Start
    ↓
1. Load bootstrap JSON (Resources/Raw/conference.json) → Display immediately
    ↓
2. Check connectivity (Connectivity.NetworkAccess)
    ↓
3. If online: Fetch remote conference.json with If-None-Match (ETag)
    ↓
4. Compare contentVersion:
   - If remote newer → Save to cache → Reload UI
   - If 304 Not Modified → Keep current
   - If error → Keep current (silent fallback)
    ↓
5. User pulls to refresh → Repeat steps 2-4
```

### Remote Endpoint Structure
```
https://confapp-static-prod.azurestaticapps.net/
├── conference.json          # Main data file
└── staticwebapp.config.json # CORS and caching config
```

### Cost Estimate
**$0/month** - Azure Static Web Apps Free tier:
- 100 GB bandwidth/month (estimated usage: ~10 GB/month)
- 0.5 GB storage
- HTTPS included

### Deployment Commands
```bash
# Login and set subscription
az login
az account set --subscription <SUBSCRIPTION_ID>

# Create resources
az group create --name rg-conferenceapp --location westeurope

az staticwebapp create \
  --name confapp-static-prod \
  --resource-group rg-conferenceapp \
  --location westeurope \
  --sku Free

# Upload content
az staticwebapp upload \
  --name confapp-static-prod \
  --resource-group rg-conferenceapp \
  --source ./wwwroot

# Get URL
az staticwebapp show \
  --name confapp-static-prod \
  --resource-group rg-conferenceapp \
  --query "defaultHostname" -o tsv
```

### Testing Plan
- [ ] Fresh install → loads bootstrap → fetches remote
- [ ] Pull to refresh → updates data
- [ ] Offline mode → uses cached data
- [ ] Network failure → silent fallback
- [ ] Outdated remote → ignores older version
- [ ] ETag 304 response → skips download

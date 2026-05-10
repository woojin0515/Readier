# Readier

Readier is an AI-assisted scheduling and preparation mobile app.

The app helps users arrive on time by analyzing:
- preparation time
- travel time
- user habits
- daily routines
- delay patterns

The goal is to reduce lateness and help users build healthier time management habits.

Readier follows a privacy-focused and local-first architecture.

---

# Core Features

## 1. Smart Schedule Planner

Users can create schedules and appointments.

Each schedule includes:
- title
- location
- start time
- estimated travel time
- estimated preparation time

The app calculates when the user should begin preparing and when they should leave.

---

## 2. Habit Analysis

The app analyzes user behavior patterns such as:
- average preparation time
- lateness frequency
- daily routine consistency
- movement and departure patterns

The app should gradually personalize recommendations over time.

All analysis should prioritize local processing whenever possible.

---

## 3. Smart Preparation Alerts

The app calculates when the user should:
- wake up
- prepare
- leave home

Notifications should adapt based on real user behavior and schedule history.

The app should remain functional even without internet access.

---

# Design Philosophy

The app should feel:
- simple
- clean
- calm
- minimal
- supportive

Avoid cluttered UI and overwhelming information.

Prioritize usability and clarity over visual complexity.

The experience should reduce stress, not create pressure.

# Mobile UI/UX Rules

Readier is a modern mobile-first application.

The UI should follow modern Android and iOS mobile UX conventions.

Avoid desktop-style layouts and generic placeholder UI.

---

## Navigation Rules

Readier uses a **drawer-only navigation pattern**, modeled on Google Calendar mobile.

- A hamburger drawer (☰) at the top-left switches between primary sections
- Primary sections (drawer destinations): Schedule, Behavior Analysis
- Secondary actions (drawer menu items): Settings, and future items like Help/About
- A single content area is shown at a time — there is no bottom tab bar

Never use:
- three-dot overflow icons (⋮) as the main navigation menu
- desktop-style toolbar layouts
- tiny touch targets
- bottom tabs in addition to the drawer (pick one — Readier uses the drawer)

The hamburger menu icon must visually look like:
☰

NOT:
⋮
or
...

---

## Reference Apps (Specific Roles)

Each reference app plays a specific role — do not mix patterns at random.

**Structure / navigation → Google Calendar (mobile)**
- Hamburger drawer for primary sections
- Single content area; no bottom tab bar
- Date-grouped agenda lists (오늘 / 내일 / specific dates as group headers)

**Visual style / event cards → Structured + TimeBloc**
- Time-leading rows: time anchored on the left in a fixed column
- Thin accent stripe between time and content
- Clean typography hierarchy: bold title, subtle location, accent-colored derived times (출발 / 준비 시작)
- Single accent color used sparingly (FAB, primary buttons, derived-time emphasis)
- Generous vertical padding, soft dark surfaces, no nested cards

**Polish / spacing → Notion Calendar**
- Generous whitespace between groups
- Single-tone elevated surfaces

The UI should NOT resemble:
- default MAUI demo apps
- Windows desktop apps
- old Android settings pages
- dense to-do apps (avoid TickTick-level density — Readier favors a calmer rhythm)

---

## Layout Rules

Use:
- card-based layouts
- rounded corners
- proper spacing
- large touch-friendly buttons
- clean typography
- soft dark mode colors

Avoid:
- cramped layouts
- tiny text
- excessive borders
- outdated button styles
- excessive text on screen

---

## Mobile UX Priorities

Prioritize:
1. fast understanding
2. minimal taps
3. one-handed usability
4. visual clarity
5. intuitive navigation

The UI should feel polished even in MVP versions.

---

## Component Rules

Preferred components:
- Hamburger drawer for primary navigation
- Floating action button (FAB) for the primary create action
- Cards for content grouping (single-tone surface, rounded corners)
- Bottom sheets for quick add (when MAUI-feasible; otherwise pushed page styled like a sheet)
- Toggle switches in settings (accent color when on)
- Date-grouped lists for agenda views

Avoid:
- bottom tabs (drawer-only per Navigation Rules)
- nested menus
- excessive popups
- desktop dropdown patterns
- tiny toolbar buttons

---

## Icon Rules

Use modern mobile icons.

The menu icon should always be:
☰

The overflow/settings icon should be:
⋮

Never confuse these two icons.

---

## AI Coding Rules

Before implementing UI:
1. Think from a mobile UX perspective
2. Follow modern Android/iOS conventions
3. Avoid default/generated MAUI-looking UI
4. Prioritize usability over technical simplicity

When uncertain, prefer modern productivity app design patterns.

---

# Technical Stack

## Frontend
- .NET MAUI
- C#
- MVVM architecture

## Local Storage
- Preferences + JSON (MVP)
- Future migration to SQLite should remain possible

## APIs
- Google Maps API
- Optional AI APIs in the future

Cloud services are NOT required for the MVP.

---

# Architecture Philosophy

Readier uses a local-first architecture.

Core app functionality should work fully offline.

User data should remain on the user's device whenever possible.

Avoid unnecessary server communication.

The app should minimize dependency on external services.

---

# Privacy Principles

- Minimize data collection
- Avoid unnecessary network requests
- Keep personal schedules and behavior data local
- Do not upload user habit data by default
- Prioritize transparency and user privacy

The app should feel trustworthy and privacy-friendly.

---

# Security Rules

- Never hardcode API keys in source code
- Use secure storage for sensitive credentials
- Separate secrets from UI/business logic
- Avoid excessive logging of user data
- Do not send personal behavioral data to external APIs unless explicitly required

Sensitive logic should be isolated into services.

---

# Project Structure

Use clean MVVM architecture.

Folders:
- Views
- ViewModels
- Models
- Services
- Helpers
- Interfaces

Business logic should never be directly written inside UI pages.

Storage logic should remain abstracted behind interfaces to allow future migration.

---

# Development Rules

- Keep code modular and maintainable
- Use async/await properly
- Avoid hardcoded values
- Write reusable services
- Use dependency injection when appropriate
- Keep ViewModels lightweight
- Separate UI logic from business logic
- Prefer simple and readable implementations

Avoid overengineering early versions.

---

# UI Rules

- Use dark-mode-friendly colors
- Avoid excessive animations
- Prioritize one-handed usability
- Keep navigation simple
- Use large touch-friendly UI elements
- Maintain consistent spacing and typography

The UI should feel lightweight and fast.

---

# MVP Goal

First milestone:

Users can:
1. create schedules
2. enter locations
3. receive leave-time recommendations
4. receive notifications
5. save schedule data locally

Focus on building stable core functionality first.

Do NOT prioritize:
- cloud sync
- accounts
- advanced AI
- social features
- complex analytics

The MVP should remain simple and reliable.

---

# Future Features

Possible future expansions:
- AI schedule optimization
- automatic habit learning
- smartwatch support
- voice assistant
- traffic prediction
- automatic wake-up scheduling
- optional cloud backup
- multi-device sync

Future features should not compromise privacy-first principles.

---

# AI Features

AI-related functionality should:
- prefer local analysis whenever possible
- minimize external API usage
- remain optional
- avoid collecting unnecessary personal data

The app should still function properly without AI features enabled.

---

# Important Notes

This app is designed primarily for:
- students
- busy people
- users struggling with time management
- users who frequently run late

The app should feel:
- helpful
- supportive
- calm
- non-judgmental

The goal is to improve habits gradually without making users feel stressed or pressured.

---

# Implementation Status

Snapshot of where the codebase stands. Update when major capabilities land or known gaps close.

## Done
- Schedule CRUD with `Schedule` model, list/edit pages, MVVM wiring, local notifications via `Plugin.LocalNotification`
- Local-first storage abstracted behind `IStorageService` (Preferences-backed JSON for MVP)
- Drawer-only navigation (Schedule / Behavior Analysis as flyout items, Settings as menu item)
- `LeaveTimeCalculator` driving "준비 시작" / "출발" derived times
- Place autocomplete + driving-route travel time via Kakao APIs (`IPlaceSearchService` + `ITravelTimeProvider`, implemented by `KakaoPlaceSearchService` / `KakaoTravelTimeProvider`)
- `ApplicationId` set to `com.readier.app` (Android package = iOS bundle ID)
- Kakao Native App Key wired through build-time env var pipeline (`KAKAO_NATIVE_APP_KEY` → generated `obj/Generated/ApiKeys.g.cs`); empty key falls back gracefully without crashing
- Settings page reduced to its true responsibilities (notifications toggle); per-schedule fields like origin / transportation / prep minutes live on the schedule edit page only

## Pending — required before Kakao APIs actually return data
- Register Android 패키지명 `com.readier.app` and iOS 번들 ID `com.readier.app` in Kakao Developers console (app ID 1453068)
- Register Android **key hash** (Base64 of debug keystore SHA-1; release keystore hash before shipping)
- Activate **카카오 내비** product in console — required for `apis-navi.kakaomobility.com` directions; without it, only Local API works
- Rotate the native app key that was pasted into chat history, then set `KAKAO_NATIVE_APP_KEY` env var on the dev machine

## Not yet built
- Behavior analysis surfaces (the `BehaviorAnalysisViewModel` exists but its data inputs and learning logic are stubs)
- Adaptive notification timing based on observed departure delays
- Preparation profile UI (model exists; no editor)
- SQLite migration path (currently Preferences + JSON)

---

# User Behavior Analysis

Readier should include a lightweight user behavior analysis system to improve preparation-time accuracy.

The goal is NOT deep personal profiling.

The goal is to estimate realistic preparation time based on simple user-selected habits and routines.

The system should remain privacy-friendly and local-first.

---

## User Preparation Profile

The app should allow users to optionally configure preparation-related behavior settings.

Possible configurable items:
- shower duration
- makeup usage
- hair styling time
- outfit selection time
- skincare routine duration
- breakfast habits
- typical preparation speed
- morning routine complexity

The app should use these values to estimate preparation time more accurately.

Avoid invasive or unnecessary questions.

All behavior-related data should remain stored locally.

---

## Adaptive Preparation Analysis

The app should gradually learn from:
- actual departure times
- notification response delays
- repeated lateness patterns
- schedule completion behavior
- average real preparation duration

The app should continuously improve recommendation accuracy over time.

The analysis should remain lightweight and explainable.

Avoid overly complex AI behavior in early versions.

---

# Transportation and Travel Analysis

Each schedule has a separate **origin (출발지)** and **destination (목적지)**, plus a selected transportation method. There is no global "default travel minutes" setting — travel time is calculated per schedule from these three inputs.

Origin and destination are entered through a place autocomplete control, not free-text only. Each picked place is stored as `{ name, address, latitude, longitude }`.

Supported transportation methods:
- 도보 (walking) — distance-based estimate (~5 km/h)
- 자전거 (bicycle) — distance-based estimate (~15 km/h)
- 대중교통 (public transit) — driving time × 1.3 approximation (rough; document as such)
- 자동차 (car) — Kakao Mobility Directions result
- 택시 (taxi) — same time as car

Settings holds a **default origin** (typical starting point, usually home) and a **default transportation method**, which auto-fill when creating a new schedule.

Future versions may auto-recommend transportation method based on distance/time of day.

---

## Travel Time Intelligence (Kakao Maps integration)

Provider: **Kakao Mobility Directions** (driving routes, `apis-navi.kakaomobility.com/v1/directions`) + **Kakao Local API** (`dapi.kakao.com/v2/local/search/...` — keyword search for autocomplete, address search as geocode fallback).

Both APIs are called over HTTPS with the header `Authorization: KakaoAK {nativeAppKey}`. The Kakao keyword search response already includes coordinates (`x`, `y`), so picking a result does not require a separate geocode call — the autocomplete result flows straight into a `Place`.

Implementation rules:
- Place autocomplete is debounced (~300ms) to limit API calls
- Travel time is calculated on-demand (button-triggered or after both places selected), never on every keystroke
- Result is written to the schedule's `EstimatedTravelMinutes`; user can manually override
- API failures fall back to the user's manual minute entry — the app must remain usable when the network is unavailable
- Keep the user's manual override sticky: do not auto-overwrite a manually edited value when origin/destination changes — only refill when the user explicitly recalculates

Travel recommendations should prioritize reliability over aggressive optimization. Reduce the risk of lateness rather than minimize every minute.

---

## API Key Handling

Keys are injected at **build time from environment variables**, not stored in source code or settings.

Required environment variable:
- `KAKAO_NATIVE_APP_KEY` — Kakao Developers 네이티브 앱 키 (single key powers both Local API and Mobility Directions)

Why the **native** app key (not REST): Kakao binds the native key to the registered Android package name + key hash and the iOS bundle ID, so a leaked key cannot be reused from another app. REST keys have no such binding and are server-only per Kakao's guidelines.

A pre-compile MSBuild target reads the env var and writes a generated source file under `obj/` (gitignored). When the env var is missing, the constant becomes an empty string and the runtime services surface a clear "키가 설정되지 않았습니다" message rather than crashing.

Before the key works on a real device, the following must be registered in Kakao Developers console for this app (ID 1453068):
- **Android 패키지명**: `com.readier.app`
- **iOS 번들 ID**: `com.readier.app`
- **Android 키 해시**: SHA-1 of the debug keystore (and release keystore before shipping), Base64-encoded
- **카카오 내비 제품 활성화**: required for `apis-navi.kakaomobility.com` directions calls; without it, only Local API works

Never commit:
- the generated keys file
- `appsettings.json` containing keys
- screenshots/logs containing keys

---

# Menu Structure

The app uses a hamburger drawer (☰) as its only navigation surface.

Drawer destinations (FlyoutItem — navigable sections with their own back stack):
- Schedule (default landing)
- Behavior Analysis

Drawer menu items (MenuItem — secondary actions):
- Settings
- (future) Notifications, Help, About

Navigation should remain simple and one-handed friendly.

Avoid excessive nested menus.

---

# Settings System

The settings page is for **app-wide, cross-schedule** preferences only. Anything that varies per schedule (origin, destination, transportation method, travel/prep minutes) belongs on the schedule edit page, not in settings.

Settings should include:
- notification preferences (toggle: enable/disable scheduling local notifications)
- privacy settings
- dark mode support
- AI feature toggles

Settings should NOT include:
- default origin / default transportation / default prep time (these are per-schedule fields, entered fresh each time a schedule is created)

Users should always remain in control of:
- their data
- notifications
- AI-related functionality

---

# AI Design Philosophy

AI in Readier should:
- assist users calmly
- avoid being intrusive
- remain transparent
- prioritize practical usefulness

The app should never feel controlling or stressful.

Recommendations should feel helpful rather than judgmental.

---

# Future AI Possibilities

Possible future AI improvements:
- automatic preparation-time estimation
- personalized schedule recommendations
- adaptive wake-up timing
- repeated lateness detection
- smart transportation suggestions

All future AI features should continue following:
- privacy-first principles
- local-first architecture
- explainable behavior
- low-stress user experience
# Copilot Instructions for Readier

## Build, test, and lint commands

This repository is a single-project .NET MAUI app (`Readier/Readier.csproj`, `Readier.sln`).

- Restore:
  - `dotnet restore Readier.sln`
- Build (solution):
  - `dotnet build Readier.sln`
- Build (Windows target on Windows dev machines):
  - `dotnet build Readier\Readier.csproj -f net9.0-windows10.0.19041.0`
- Run (Windows target):
  - `dotnet build Readier\Readier.csproj -t:Run -f net9.0-windows10.0.19041.0`

Tests/lint:
- There are currently no test projects in this repository (`*Test*.csproj` / `*Tests*.csproj` not present).
- There is no dedicated lint command/config checked in (no `.editorconfig`, `Directory.Build.props`, `stylecop.json`, etc.).

Environment variables:
- Kakao APIs are wired from `KAKAO_NATIVE_APP_KEY` at build time into `obj/Generated/ApiKeys.g.cs` (do not commit generated key files).

## High-level architecture

Readier is a local-first MAUI MVVM app with clear layer boundaries:

- **Domain** (`Readier/Domain`)
  - Core models (`Schedule`, `Place`, `TransportationMode`, `TravelTimeEstimate`, etc.)
  - Service interfaces (`IStorageService`, `IUserPreferencesService`, `IPlaceSearchService`, `IScheduleNotificationService`, `ILeaveTimeCalculator`)
- **Infrastructure** (`Readier/Infrastructure`)
  - Storage: `PreferencesStorageService` serializes JSON into `Preferences`
  - Prediction: `LeaveTimeCalculator` derives `StartPrepAt` and `LeaveAt`
  - External/Kakao: `KakaoPlaceSearchService` + `KakaoTravelTimeProvider`
  - Notifications: `LocalNotificationService` via `Plugin.LocalNotification`
- **Presentation** (`Readier/Presentation`)
  - `Schedules`, `Insights`, `Settings`, `Controls`
  - ViewModels use CommunityToolkit MVVM attributes (`[ObservableProperty]`, `[RelayCommand]`)

Composition root:
- `MauiProgram.cs` registers all services, viewmodels, and pages with DI.

Navigation:
- `AppShell.xaml` enforces drawer-only navigation:
  - Flyout sections: 일정 (Schedule), 분석 (Behavior Analysis)
  - Menu item: 설정 (Settings)
  - Route-based edit/settings navigation in `AppShell.xaml.cs`

Data flow (schedule path):
1. `ScheduleEditViewModel` gathers title/origin/destination/time/travel/prep/transportation.
2. Optional travel-time fetch uses `ITravelTimeProvider` (Kakao + transport-mode conversion rules).
3. Save persists to storage key `readier.schedules.v1`.
4. Notification scheduling is re-applied through `IScheduleNotificationService`.
5. `ScheduleListViewModel` reloads schedules, computes leave/prep plan, and groups by date labels (`오늘`, `내일`, etc.).

## Key repository conventions

- **Production shipping quality is mandatory**: treat this app as a real release target, not a prototype. Changes should raise overall polish (UI quality, interaction consistency, reliability), not just “make it work.”
- **Design-first default for future work**: most upcoming tasks are expected to be UI/UX design work. When in doubt, prioritize visual quality, spacing rhythm, touch ergonomics, and cohesive interaction patterns over quick scaffolding.
- **Benchmark-driven product decisions**: for structure/UX decisions, actively reference successful adjacent apps (Google Calendar, Structured, TimeBloc, Notion Calendar, Todoist, TickTick) and adapt patterns to Readier’s drawer-only, low-stress design language.
- **iOS lock-screen surface planning**: include iOS Live Activities / Dynamic Island fit in notification-related design discussions (when meaningful for schedule/reminder flow) while keeping Android parity in mind.
- **Privacy/local-first defaults**: core behavior must remain usable offline; network/API failures should degrade gracefully to manual input rather than blocking schedule creation.
- **Build-time API key injection**: `KAKAO_NATIVE_APP_KEY` is read by an MSBuild target; missing key should produce safe, user-facing fallback behavior (not app crashes).
- **Versioned storage keys**: use stable, versioned keys for persisted payloads (`readier.schedules.v1`, `readier.preferences.v1`).
- **Notification semantics**: always cancel existing per-schedule notifications before re-scheduling; two notifications are used per schedule (`prep`, `leave`).
- **Travel-time UX**: place lookup is debounced (~300ms) and travel estimation is explicit (button-triggered), not keystroke-triggered.
- **UI/UX contract**:
  - Drawer-only primary navigation (no bottom tabs)
  - Mobile-first dark-friendly card surfaces and accent-driven hierarchy
  - Keep strings/user-facing copy aligned with existing Korean UI text
- **Code organization expectation**: business logic should stay in services/viewmodels, not in page code-behind; code-behind is mainly binding/lifecycle wiring (`OnAppearing` loads viewmodel data).

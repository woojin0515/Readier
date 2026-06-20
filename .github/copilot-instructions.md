# Copilot Instructions for Readier

## Build, test, and lint commands

This repository is a web-based application consisting of:

* ASP.NET Core Backend API
* Blazor Web App / Razor Components Frontend (or equivalent web UI)

Build & Run:

* Restore:

  * `dotnet restore Readier.sln`
* Build:

  * `dotnet build Readier.sln`
* Run backend:

  * `dotnet run --project Readier.Api`
* Run frontend:

  * `dotnet run --project Readier.Web`

Tests/lint:

* There are currently no test projects in this repository.
* There is no dedicated lint configuration checked in.
* Follow standard .NET analyzers and maintain clean build output with zero warnings whenever possible.

Environment variables:

* Kakao APIs use `KAKAO_REST_API_KEY`
* OpenAI APIs use `OPENAI_API_KEY`
* Secrets must never be committed to source control.
* Use environment variables or secure secret storage for all production credentials.

---

## High-level architecture

Readier is a web-first schedule planning platform designed to help users leave at the optimal time and avoid being late.

### Domain (`Domain`)

Core business entities:

* Schedule
* Place
* TransportationMode
* TravelTimeEstimate
* UserPreferences
* BehaviorInsights

Service interfaces:

* IStorageService
* IUserPreferencesService
* IPlaceSearchService
* ITravelTimeProvider
* ILeaveTimeCalculator
* IInsightService

---

### Infrastructure (`Infrastructure`)

Persistence:

* SQLite (development)
* PostgreSQL (production)

External services:

* Kakao Local API
* Kakao Mobility API
* OpenAI API

Caching:

* MemoryCache
* Distributed cache (future)

---

### Application (`Application`)

Contains:

* Use cases
* Business rules
* Validation
* DTOs
* Service orchestration

Business logic must remain framework-independent.

---

### Presentation (`Web`)

Responsibilities:

* UI Components
* Pages
* Routing
* State Management

Guidelines:

* Responsive design for desktop, tablet, and mobile.
* Mobile experience remains a priority.
* Progressive Web App (PWA) support is expected.
* Dark mode support is required.

---

## Data Flow (Schedule Path)

1. User creates a schedule.
2. User selects origin and destination.
3. Travel time is estimated through Kakao APIs.
4. Leave time and preparation time are calculated.
5. Schedule is stored in the database.
6. Dashboard and analytics are updated.
7. Browser notifications may be scheduled when supported.

---

## Key Repository Conventions

### Production Quality

This project targets real-world users.

Changes should improve:

* Reliability
* Performance
* Accessibility
* Design consistency
* User trust

Avoid prototype-style implementations.

---

### Design-First Product Development

Most future work is expected to focus on:

* UI quality
* UX polish
* Interaction consistency
* Responsive layouts

Reference successful products such as:

* Google Calendar
* Structured
* TimeBloc
* Notion Calendar
* TickTick
* Todoist

Adapt patterns rather than copying them.

---

### Web-First Philosophy

Readier is now a web application.

Prioritize:

* Fast loading
* Responsive layouts
* PWA compatibility
* SEO-friendly public pages where applicable
* Accessibility (WCAG considerations)

Do not make decisions that depend on mobile app stores, native mobile APIs, or platform-specific mobile features unless explicitly requested.

---

### Privacy and Resilience

Core functionality should continue working even when:

* Kakao APIs fail
* OpenAI APIs are unavailable
* Network latency increases

Provide graceful fallbacks whenever possible.

Users should never lose schedule data due to external service failures.

---

### Versioned Persistence

Use stable versioned schemas and keys:

* `readier.schedules.v1`
* `readier.preferences.v1`

Future migrations should preserve backward compatibility whenever possible.

---

### Travel-Time UX

Guidelines:

* Place search should be debounced (~300ms).
* Travel estimation should be user-triggered.
* Manual overrides must always be available.
* Users should never be blocked by API failures.

---

### UI / UX Contract

Primary design goals:

* Calm and low-stress experience
* Clean information hierarchy
* Modern card-based layouts
* Excellent mobile usability
* Responsive desktop experience
* Dark mode support

Keep all user-facing copy aligned with existing Korean terminology.

---

### Code Organization Expectations

Business logic belongs in:

* Domain
* Application
* Services

UI logic belongs in:

* Components
* Pages

Avoid placing business rules inside UI components.

Maintain clear separation between:

* Domain
* Application
* Infrastructure
* Presentation

Prefer dependency injection and service abstractions throughout the codebase.

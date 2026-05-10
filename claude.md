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
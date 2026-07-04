# Readier Build Guide

## ⚠️ IMPORTANT: Android SDK Setup Required

Before deploying to Galaxy A32, you **MUST** complete Android Studio initial setup to download SDKs and tools.

**The Android SDK hasn't been automatically downloaded yet.** Follow the Android Studio setup section below.

---

## 📋 Quick Status Check

Run this to verify your build environment:

```powershell
cd C:\Users\최승규\source\repos\woojin0515\Readier
.\verify-build-setup.ps1
```

This script checks:
- ✅ Java JDK 21 LTS
- ✅ Android Studio installation
- ⚠️ Android SDK (may need download)
- ✅ .NET SDK
- ✅ Readier project files

---

## Environment Setup

Java and Android Studio are installed, but Android SDK setup requires manual completion:

- **Java JDK 21 LTS** (Eclipse Adoptium) → `C:\Program Files\Eclipse Adoptium\jdk-21.0.11.10-hotspot`
- **Android Studio** → `C:\Program Files\Android\Android Studio`
- **Environment Variables**:
  - `JAVA_HOME` = `C:\Program Files\Eclipse Adoptium\jdk-21.0.11.10-hotspot`
  - `ANDROID_HOME` = `C:\Users\최승규\AppData\Local\Android\Sdk`
  - `PATH` includes both

## Building from Command Line (CLI)

### Windows, iOS, macOS (Recommended)
```powershell
dotnet build Readier.sln
```
✅ **Works perfectly** - builds all three platforms.

### Android (GUI Only - See Below)
⚠️ **AAPT2 limitation**: CLI builds for Android fail due to a known .NET MAUI asset packaging issue on Windows. 
**Workaround**: Use Android Studio (see below).

## 🔧 Android Studio Initial Setup (REQUIRED)

**IMPORTANT:** Android Studio needs to download SDKs and build tools before you can build Android apps. This is a one-time setup.

### Step 1: Launch Android Studio Initial Setup

```powershell
& 'C:\Program Files\Android\Android Studio\bin\studio64.exe'
```

### Step 2: Complete Initial Wizard

1. Android Studio will show a setup wizard
2. **Accept** all default suggestions (or skip if prompted)
3. The wizard will download:
   - Android SDK 35 (and other platforms)
   - Android SDK Build-Tools
   - Android Emulator
   - NDK (optional, not required for Readier)

⏱️ **This may take 5-20 minutes** depending on your internet speed. Let it complete fully.

### Step 3: Verify SDK Installation

Once Android Studio finishes setup, verify the SDK was installed:

```powershell
Get-ChildItem "C:\Users\최승규\AppData\Local\Android\Sdk" | Select-Object -First 10
```

Expected output: `platforms/`, `build-tools/`, `cmdline-tools/`, etc.

---

## Building for Galaxy A32 (Recommended)

### Method 1: Android Studio GUI ✅ (RECOMMENDED)

**After completing the setup above, do this to deploy to Galaxy A32:**

1. Open Android Studio (if not already open):
   ```powershell
   & 'C:\Program Files\Android\Android Studio\bin\studio64.exe'
   ```

2. Open the Readier project:
   - File → Open → Navigate to `C:\Users\최승규\source\repos\woojin0515\Readier`
   - Select the folder and click Open
   - Android Studio will recognize it as a MAUI project

3. Wait for Gradle sync to complete
   - A progress bar will appear at the bottom
   - Let it finish fully (this indexes the project)

4. Connect Galaxy A32 via USB:
   - Enable Developer Mode: Settings → About phone → Build number (tap 7 times)
   - Enable USB Debugging: Settings → Developer options → USB Debugging
   - Connect USB cable to computer

5. Build and run:
   - Run → Run 'app' (or press Shift+F10)
   - Select Galaxy A32 from the device list
   - APK will compile, install, and launch automatically

### Method 2: CLI with Flag (If AAPT2 Fixed)

If AAPT2 issues are resolved in a future .NET SDK:

```powershell
dotnet build Readier.sln -p:IncludeAndroid=true
# or for direct publishing:
dotnet publish Readier -f net9.0-android35.0 -c Release
```

## Build Outputs

After running `dotnet build Readier.sln`:

```
Readier\bin\Debug\
├── net9.0-windows10.0.19041.0\win10-x64\Readier.exe      ← Windows app
├── net9.0-ios18.0\iossimulator-x64\Readier.dll           ← iOS simulator
└── net9.0-maccatalyst18.0\maccatalyst-x64\Readier.dll    ← macOS Catalyst
```

For Android (built via Android Studio):
```
Readier\bin\Debug\net9.0-android35.0\
├── com.readier.app-debug.apk     ← Install to Galaxy A32
└── ...
```

## Testing on Galaxy A32

1. **Enable Developer Mode** on Galaxy A32:
   - Settings → About phone → Build number (tap 7 times)
   - Settings → Developer options → USB Debugging (enable)

2. **Connect via USB**

3. **Use Android Studio to deploy**:
   - Run → Run 'app'
   - Select Galaxy A32
   - APK will auto-install and launch

## Troubleshooting

| Issue | Solution |
|-------|----------|
| `java: command not found` | Restart terminal or add `C:\Program Files\Eclipse Adoptium\jdk-21.0.11.10-hotspot\bin` to PATH manually |
| `ANDROID_HOME not found` | Restart Android Studio or set env var: `$env:ANDROID_HOME = 'C:\Users\최승규\AppData\Local\Android\Sdk'` |
| Galaxy A32 not detected in Android Studio | Enable USB Debugging on device; try different USB port or cable |
| AAPT2 error in CLI builds | This is expected on Windows. Use Android Studio instead. |

## Project Structure

Single-project MAUI app with platform-specific targets:

```
Readier.csproj
├── TargetFrameworks (conditional):
│   ├── Windows: net9.0-windows10.0.19041.0
│   ├── iOS: net9.0-ios18.0
│   ├── macOS: net9.0-maccatalyst18.0
│   └── Android: net9.0-android35.0 (excluded from default build)
```

**Why Android is excluded by default**: AAPT2 (Android Asset Packaging Tool) on Windows has a known limitation where it fails to locate the assets directory during CLI builds, even though the same code compiles fine in Android Studio's Gradle system.

## Environment Variables at Build Time

The app reads the `KAKAO_NATIVE_APP_KEY` environment variable during build:

```
KAKAO_NATIVE_APP_KEY = "your-native-app-key-here"
```

This key is injected into `obj\Generated\ApiKeys.g.cs` (gitignored). If the env var is missing:
- The app will build successfully
- At runtime, place search and travel time features will show: "키가 설정되지 않았습니다" (key not configured)

## Environment Variables at Runtime (Auth + DB)

For cross-device sync and Google social login, set these runtime variables:

```
READIER_DB_CONNECTION="Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<db>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
GOOGLE_CLIENT_ID="<google-oauth-client-id>"
GOOGLE_CLIENT_SECRET="<google-oauth-client-secret>"
```

- `READIER_DB_CONNECTION` is used for Azure SQL persistence.
- If DB connection is missing, the app falls back to browser localStorage only.
- If Google OAuth keys are missing, `/auth/login` redirects back to `/` without login.

## Next Steps

1. ✅ **CLI builds**: `dotnet build Readier.sln` (Windows, iOS, macOS)
2. ✅ **Android deployment**: Open in Android Studio → Run to Galaxy A32
3. 🔜 **Register app in Kakao Developers**: Set `KAKAO_NATIVE_APP_KEY` env var for full API access

---

**Last updated**: 2026-06-19  
**Built with**: .NET 9.0, MAUI, C#

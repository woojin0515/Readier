# Readier Build Environment Verification Script
# Run this to check if your system is ready to build

Write-Host ""
Write-Host "╔═════════════════════════════════════════════════════════════╗"
Write-Host "║         Readier Build Environment Verification              ║"
Write-Host "╚═════════════════════════════════════════════════════════════╝"
Write-Host ""

# Color helpers
function Write-Success { Write-Host "✅ $args" -ForegroundColor Green }
function Write-Error { Write-Host "❌ $args" -ForegroundColor Red }
function Write-Warn { Write-Host "⚠️  $args" -ForegroundColor Yellow }
function Write-Info { Write-Host "ℹ️  $args" -ForegroundColor Cyan }

$allGood = $true

# 1. Check Java
Write-Host ""
Write-Host "1️⃣  Checking Java..."
$javaHome = $env:JAVA_HOME
if (-not $javaHome) {
    $javaHome = (Get-ItemProperty "HKCU:\Environment" -ErrorAction SilentlyContinue).JAVA_HOME
}

if ($javaHome -and (Test-Path $javaHome)) {
    Write-Success "JAVA_HOME = $javaHome"
    
    # Verify java command works in new shell
    $javaPath = "$javaHome\bin\java.exe"
    if (Test-Path $javaPath) {
        Write-Success "java.exe found"
        $javaVersion = & $javaPath -version 2>&1 | Select-Object -First 1
        Write-Info $javaVersion
    } else {
        Write-Error "java.exe not found at $javaPath"
        $allGood = $false
    }
} else {
    Write-Error "JAVA_HOME not set or invalid"
    $allGood = $false
}

# 2. Check Android Studio
Write-Host ""
Write-Host "2️⃣  Checking Android Studio..."
$studioPath = "C:\Program Files\Android\Android Studio\bin\studio64.exe"
if (Test-Path $studioPath) {
    Write-Success "Android Studio found: $studioPath"
} else {
    Write-Error "Android Studio not found at $studioPath"
    $allGood = $false
}

# 3. Check Android SDK
Write-Host ""
Write-Host "3️⃣  Checking Android SDK..."
$androidHome = $env:ANDROID_HOME
if (-not $androidHome) {
    $androidHome = (Get-ItemProperty "HKCU:\Environment" -ErrorAction SilentlyContinue).ANDROID_HOME
}

if ($androidHome) {
    Write-Info "ANDROID_HOME = $androidHome"
    
    if (Test-Path $androidHome) {
        Write-Success "Android SDK path exists"
        
        $platforms = Get-ChildItem "$androidHome\platforms" -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count
        $buildTools = Get-ChildItem "$androidHome\build-tools" -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count
        
        if ($platforms -gt 0) {
            Write-Success "Found $platforms Android platform(s)"
        } else {
            Write-Warn "No Android platforms installed - run Android Studio setup to download"
            $allGood = $false
        }
        
        if ($buildTools -gt 0) {
            Write-Success "Found $buildTools build-tools version(s)"
        } else {
            Write-Warn "No build-tools installed - run Android Studio setup to download"
            $allGood = $false
        }
    } else {
        Write-Warn "Android SDK not yet downloaded"
        Write-Info "Run Android Studio first-launch setup to download SDKs:"
        Write-Info "  & '$studioPath'"
        $allGood = $false
    }
} else {
    Write-Error "ANDROID_HOME not set"
    $allGood = $false
}

# 4. Check .NET CLI
Write-Host ""
Write-Host "4️⃣  Checking .NET..."
$dotnetVersion = dotnet --version 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Success ".NET SDK: $dotnetVersion"
} else {
    Write-Error ".NET SDK not found"
    $allGood = $false
}

# 5. Check project files
Write-Host ""
Write-Host "5️⃣  Checking Readier project..."
$csprojPath = "$(pwd)\Readier\Readier.csproj"
$slnPath = "$(pwd)\Readier.sln"

if (Test-Path $csprojPath) {
    Write-Success "Readier.csproj found"
} else {
    Write-Error "Readier.csproj not found (are you in the repo root?)"
    $allGood = $false
}

if (Test-Path $slnPath) {
    Write-Success "Readier.sln found"
} else {
    Write-Error "Readier.sln not found"
    $allGood = $false
}

# Final summary
Write-Host ""
Write-Host "╔═════════════════════════════════════════════════════════════╗"

if ($allGood) {
    Write-Host "║         ✅ All systems ready for building!                  ║"
    Write-Host "╚═════════════════════════════════════════════════════════════╝"
    Write-Host ""
    Write-Host "Next steps:"
    Write-Host "  1. CLI build: dotnet build Readier.sln"
    Write-Host "  2. Android (if SDK is set up): Open Android Studio → Run 'app'"
} else {
    Write-Host "║         ⚠️  Setup incomplete - see warnings above            ║"
    Write-Host "╚═════════════════════════════════════════════════════════════╝"
    Write-Host ""
    Write-Host "Most common issue: Android SDK not downloaded"
    Write-Host "  → Run Android Studio first-launch setup:"
    Write-Host "     & 'C:\Program Files\Android\Android Studio\bin\studio64.exe'"
}
Write-Host ""

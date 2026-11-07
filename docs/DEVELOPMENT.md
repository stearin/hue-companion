# Development Workflow

This document describes the repository’s development, testing, and release configuration, reviewed against source on 2026-11-07. Workflow configuration does not establish that a release or Store submission has succeeded.

## Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         DEVELOPMENT WORKFLOW                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. DEVELOP          2. TEST            3. REVIEW         4. RELEASE    │
│  ─────────           ─────              ──────            ───────       │
│                                                                          │
│  Write code    →    Run tests    →    Create PR    →    Bump version   │
│  Local build        Local coverage     CI validates      Tag & push     │
│                                        Code review       CI releases    │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

## 1. Local Development

### Build Commands

Run desktop commands from the repository root on Windows with the .NET SDK and WinUI/MSIX build tooling installed.

```powershell
# Preferred development workflow
.\hue.cmd dev
.\hue.cmd dev -Restart
.\hue.cmd dev -NoLaunch

# Direct architecture-specific builds
dotnet build src/HueCompanion/HueCompanion.csproj -p:Platform=x64
dotnet build src/HueCompanion/HueCompanion.csproj -p:Platform=ARM64

# Build the separate bridge-control MCP executable
dotnet build src/HueCompanion.Mcp/HueCompanion.Mcp.csproj
```

For Visual Studio packaged debugging, use the `MsixPackage` launch profile described in [README.md](../README.md#build-from-command-line). Plain `dotnet run` is not the documented packaged deployment workflow. The website has a separate [npm workflow](../website/README.md).

### Project Structure

```
src/
├── HueCompanion/           # WinUI 3 App (views, controls, styles)
├── HueCompanion.Core/      # Business logic (models, viewmodels, services)
├── HueCompanion.Mcp/       # Bridge-control MCP host and tools
└── HueCompanion.Tests/     # xUnit v3 tests for Core and MCP helpers
```

## 2. Testing

### Running Tests Locally

```bash
# Run all tests
dotnet test src/HueCompanion.Tests

# Run with verbose output
dotnet test src/HueCompanion.Tests -v normal

# Run specific test class
dotnet test src/HueCompanion.Tests --filter "FullyQualifiedName~HueColorTests"

# Run with code coverage
dotnet test src/HueCompanion.Tests --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

### Test Categories

| Category | Location | Description |
|----------|----------|-------------|
| **Models** | `src/HueCompanion.Tests/Models/` | Color conversion and Result types |
| **Services** | `src/HueCompanion.Tests/Services/` | Storage, settings, animation, easing, scene assignments |
| **ViewModels** | `src/HueCompanion.Tests/ViewModels/` | Scene Builder state and commands |
| **MCP** | `src/HueCompanion.Tests/Mcp/` | Color parsing and fuzzy matching |

### Test Patterns

- **FluentAssertions** for readable assertions
- **IDisposable** pattern for test cleanup
- **Production services with temporary-directory constructor overrides** for file I/O tests
- **xUnit v3 cancellation**: pass `TestContext.Current.CancellationToken` to cancellable operations
- **Moq expression trees**: supply all arguments, including optional parameters
- **AAA pattern** (Arrange, Act, Assert)

### Coverage Reports

After running tests with coverage, reports are in `TestResults/*/coverage.cobertura.xml`.

View coverage in CI via Codecov: https://codecov.io/gh/ddrayne/hue-companion

## 3. Continuous Integration

### CI Workflows

| Workflow | Trigger | What it does |
|----------|---------|--------------|
| **PR Validation** | Pull requests to main | Build, tests/coverage, screenshot and visual checks |
| **Main CI** | Push to main | x64/ARM64 builds, tests/coverage, screenshot and visual checks, package artifacts |

### PR Validation Flow

```
PR opened/updated
       │
       ▼
┌──────────────────┐
│  Checkout code   │
│  Setup .NET 8.0  │
│  Restore deps    │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Build (Release) │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│   Run tests      │
│   with coverage  │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Upload artifacts │
│ Upload to Codecov│
└──────────────────┘
```

### Quality Gates

- All tests must pass
- Coverage uploaded to Codecov
- Codecov status targets: 50% project, 70% patch, with a 5% threshold in `codecov.yml`. No local minimum coverage threshold is enforced; required external status checks depend on repository branch protection.
- UI screenshot and visual-comparison steps currently use `continue-on-error: true`.

## 4. Versioning

### Semantic Versioning

Format: `MAJOR.MINOR.PATCH`

- **MAJOR**: Breaking changes or major milestones
- **MINOR**: New features (backwards compatible)
- **PATCH**: Bug fixes

### Version Files

The bump script updates release version values in two locations:

| File | Format | Example |
|------|--------|---------|
| `src/HueCompanion/HueCompanion.csproj` | `<Version>X.Y.Z</Version>` | `<Version>1.0.0</Version>` |
| `src/HueCompanion/Package.appxmanifest` | `Version="X.Y.Z.0"` | `Version="1.0.0.0"` |

The project also has `BaseVersion=0.1.0` for informational versions and CLI-supplied dev/stable suffixes; `Package.Dev.appxmanifest` has a separate development identity. The release bump script does not synchronize those values.

### Version Bump Script

```powershell
# Preview changes (dry run)
.\tools\Bump-Version.ps1 -DryRun

# Bump versions
.\tools\Bump-Version.ps1              # Patch: 1.0.0 → 1.0.1
.\tools\Bump-Version.ps1 -Type minor  # Minor: 1.0.0 → 1.1.0
.\tools\Bump-Version.ps1 -Type major  # Major: 1.0.0 → 2.0.0

# With custom release message
.\tools\Bump-Version.ps1 -Message "Added dark mode support"

# Auto-push after bumping (triggers release workflow)
.\tools\Bump-Version.ps1 -Push
```

The script:
1. Reads current version from git tags
2. Calculates new version
3. Updates .csproj and Package.appxmanifest
4. Creates git commit
5. Creates annotated git tag `vX.Y.Z`
6. Optionally pushes to origin (with `-Push`)

## 5. Release Process

### Release Workflow

When a tag matching `v*.*.*` is pushed, the release workflow automatically:

```
Tag pushed (v1.2.3)
       │
       ▼
┌──────────────────┐
│   Run tests      │  ← Must pass before building
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Build packages  │  ← x64 and ARM64 in parallel
│  (matrix build)  │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Create GitHub    │  ← Automatic release with:
│    Release       │     - Release notes
└────────┬─────────┘     - x64 zip attached
         │               - ARM64 zip attached
         ▼
    Release published!
```

### Creating a Release

**Quick release (recommended):**
```powershell
# Bump and push in one command
.\tools\Bump-Version.ps1 -Type patch -Push
```

**Manual release:**
```powershell
# 1. Ensure clean state
git status
dotnet test src/HueCompanion.Tests

# 2. Bump version
.\tools\Bump-Version.ps1 -Type minor

# 3. Push to trigger workflow
git push origin main --tags
```

### Release Checklist

- [ ] All tests pass locally (`dotnet test src/HueCompanion.Tests`)
- [ ] Working directory is clean (`git status`)
- [ ] Version bump executed (`.\tools\Bump-Version.ps1`)
- [ ] Tag pushed to origin
- [ ] Release workflow passed (check GitHub Actions)
- [ ] GitHub Release created with artifacts

### Monitoring Releases

After pushing a tag:
1. Go to https://github.com/ddrayne/hue-companion/actions
2. Watch the "Release" workflow
3. Once complete, check https://github.com/ddrayne/hue-companion/releases

### Failed Release Recovery

If the release workflow fails:

```bash
# 1. Delete the local and remote tag
git tag -d v1.2.3
git push origin :refs/tags/v1.2.3

# 2. Fix the issue

# 3. Re-run version bump (will recreate same version)
.\tools\Bump-Version.ps1 -Push
```

### Hotfix Process

For urgent fixes on released versions:

```bash
# 1. Checkout the release tag
git checkout v1.0.0

# 2. Create hotfix branch
git checkout -b hotfix/1.0.1

# 3. Make fix, commit
git commit -m "fix: critical bug"

# 4. Bump patch version and push
.\tools\Bump-Version.ps1 -Push
```

## 6. Configuration Files

### codecov.yml

Controls Codecov behavior:

```yaml
coverage:
  status:
    project:
      default:
        target: 50%      # Overall coverage target
        threshold: 5%    # Allow 5% decrease
    patch:
      default:
        target: 70%      # New code coverage target
        threshold: 5%

ignore:
  - "**/*.xaml.cs"       # UI code-behind
  - "**/obj/**"          # Build artifacts
  - "**/bin/**"
```

### Directory.Build.props

Shared build settings for all projects:

```xml
<PropertyGroup>
  <LangVersion>latest</LangVersion>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

## 7. Troubleshooting

### Tests Fail Locally but Pass in CI

- Check .NET SDK version matches (`dotnet --version`)
- Ensure clean build: `dotnet clean && dotnet build`
- Check for environment-specific paths

### Coverage Not Uploading to Codecov

- Verify CODECOV_TOKEN secret is set in GitHub
- Check coverage file exists: `TestResults/**/coverage.cobertura.xml`
- Codecov failures don't break CI (`fail_ci_if_error: false`)

The project also has `BaseVersion=0.1.0` for informational versions and CLI-supplied dev/stable suffixes; `Package.Dev.appxmanifest` has a separate development identity. The release bump script does not synchronize those values.

### Version Bump Script Doesn't Find Tags

- Ensure tags are fetched: `git fetch --tags`
- Check tag format matches `v*.*.*` pattern
- Try `git tag -l "v*.*.*"` to list matching tags

### Build Fails for ARM64

- Some NuGet packages may not support ARM64
- Check Package.appxmanifest for correct TargetDeviceFamily
- Try x64 build first to isolate platform-specific issues

## 8. UI & Visual Regression Testing

### Running UI Tests Locally

```powershell
# Full build + capture all test pages
.\tools\Run-UITests.ps1

# Quick capture (skip build)
.\tools\Run-UITests.ps1 -SkipBuild

# Test specific pages
.\tools\Run-UITests.ps1 -SkipBuild -Pages dashboard,settings
```

Screenshots are saved to `test-screenshots/`.

### Visual Regression Testing

```powershell
# Compare current screenshots against baselines
.\tools\Compare-Screenshots.ps1

# Update baselines (after reviewing changes)
.\tools\Compare-Screenshots.ps1 -UpdateBaselines

# Use stricter threshold (default is 1%)
.\tools\Compare-Screenshots.ps1 -Threshold 0.5
```

**Threshold:** 1% pixel difference allowed by default.

### Baseline Management

Baseline screenshots are stored in `tests/visual-baselines/`. When UI changes are intentional:

1. Run UI tests: `.\tools\Run-UITests.ps1`
2. Review screenshots in `test-screenshots/`
3. Update baselines: `.\tools\Compare-Screenshots.ps1 -UpdateBaselines`
4. Commit: `git add tests/visual-baselines/*.png`

### CI Integration

| Workflow | What it does |
|----------|--------------|
| **Main CI** | Captures screenshots, runs regression tests, uploads artifacts |
| **PR Validation** | Compares screenshots, comments on PR if differences detected |

**Artifacts:** Screenshots and diff images are uploaded for review when tests run.

### Testable Pages

| Page | Command |
|------|---------|
| Dashboard | `--page dashboard` |
| Settings | `--page settings` |
| Room (with bridge) | `--page room --name <room-name>` |
| Zone (with bridge) | `--page zone --name <zone-name>` |
| Light (with bridge) | `--page light --name <light-name>` |

**Note:** Pages requiring room/zone/light need a connected Hue bridge.

## 9. MSIX Packaging

### Local Development (Self-Signed)

For local testing with signed MSIX packages:

```powershell
# Create a self-signed certificate (one-time)
.\tools\Create-SelfSignedCert.ps1

# Optionally export to file
.\tools\Create-SelfSignedCert.ps1 -OutputPath .\dev-cert.pfx

# Build with signing (if needed)
dotnet publish src/HueCompanion/HueCompanion.csproj `
  -p:Platform=x64 `
  -p:AppxPackageSigningEnabled=true `
  -p:PackageCertificateThumbprint=<thumbprint>
```

### Production Signing Options

For signed releases:

1. **EV Code Signing Certificate** - Purchase from a trusted CA (DigiCert, Sectigo)
2. **Windows Store Signing** - Automatic when publishing to Microsoft Store
3. **Self-Signed (Dev Only)** - Requires the signing certificate to be trusted on the test machine; Developer Mode alone does not establish certificate trust

### Release Workflow

The automated release workflow:
1. Builds for x64 and ARM64
2. Creates ZIP archives and unsigned MSIX packages (`AppxPackageSigningEnabled=false`)
3. Generates release notes
4. Creates a GitHub Release with ZIP and MSIX artifacts attached
5. Optionally runs the Store submission job when enabled

To create a release:
```powershell
.\tools\Bump-Version.ps1 -Push
```

### Artifact Structure

The workflow prepares both ZIP and MSIX formats. The app declares MSIX packaging; a ZIP is not proof that launching outside a package works. Validate deployment, runtime prerequisites, and signing before distributing sideload instructions:

```
HueCompanion-{version}-{platform}.zip     # Published app files; verify runtime/package prerequisites
├── HueCompanion.exe
├── HueCompanion.dll
├── *.dll
└── Assets/

HueCompanion-{version}-{platform}.msix    # Installable package
```

## 10. Microsoft Store Deployment

### Overview

The release workflow can automatically publish to the Microsoft Store when configured.

```
Push tag v0.1.0
       │
       ▼
┌──────────────┐
│  Run Tests   │
└──────┬───────┘
       │
       ▼
┌──────────────────────┐
│  Build MSIX + ZIP    │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  GitHub Release      │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  STORE_ENABLED?      │──no──> Done
└──────┬───────────────┘
       │ yes
       ▼
┌──────────────────────┐
│  Publish to Store    │
└──────────────────────┘
```

### Setup

To enable Store deployment, follow the setup guide: [STORE-SETUP.md](STORE-SETUP.md)

**Required secrets:**
- `STORE_TENANT_ID` - Azure AD tenant ID
- `STORE_CLIENT_ID` - Azure AD application ID
- `STORE_CLIENT_SECRET` - Azure AD client secret
- `STORE_PRODUCT_ID` - Partner Center product ID

**Required variable:**
- `STORE_ENABLED` = `true`

### Enabling/Disabling

Store deployment is controlled by the `STORE_ENABLED` repository variable:

- Set to `true` → Releases publish to Store
- Set to `false` or delete → Releases only go to GitHub

### Troubleshooting

| Issue | Solution |
|-------|----------|
| Store job doesn't run | Check the `STORE_ENABLED` repository variable and the workflow condition |
| Authentication failed | Verify secrets match Azure AD app |
| Package validation failed | Check MSIX manifest matches Partner Center identity |
| Version rejected | Ensure version is higher than current Store version |

For detailed troubleshooting, see [STORE-SETUP.md](STORE-SETUP.md#troubleshooting).

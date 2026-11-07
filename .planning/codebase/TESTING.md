# Testing Patterns

**Analysis Date:** 2026-11-07

## Test Framework

**Runner:**
- xUnit v3 (`xunit.v3` 4.0.2)
- Config: `src/HueCompanion.Tests/HueCompanion.Tests.csproj`

**Assertion Library:**
- FluentAssertions 8.11.0

**Run Commands:**
```bash
dotnet test src/HueCompanion.Tests/HueCompanion.Tests.csproj           # Run all tests
dotnet watch --project src/HueCompanion.Tests/HueCompanion.Tests.csproj test   # Watch mode
dotnet test src/HueCompanion.Tests/HueCompanion.Tests.csproj --collect:"XPlat Code Coverage"  # Coverage
```

## Test File Organization

**Location:**
- Co-located in separate `HueCompanion.Tests` project following source structure
- Tests mirror source namespace structure: `HueCompanion.Tests.Models`, `HueCompanion.Tests.Services`

**Naming:**
- Test classes: `[ClassName]Tests` (e.g., `HueColorTests`, `ResultTests`, `SceneStorageServiceTests`)
- Test methods: `[MethodName]_[Scenario]_[ExpectedResult]` pattern observed
  - Example: `Constructor_ClampsXToValidRange`, `SaveSceneAsync_CreatesJsonFile`, `FromRgb_RoundTripsApproximately`

**Structure:**
```text
src/HueCompanion.Tests/
|-- Mcp/
|   |-- ColorParserTests.cs
|   `-- FuzzyMatcherTests.cs
|-- Models/
|   |-- ColorConverterTests.cs
|   |-- HueColorTests.cs
|   `-- ResultTests.cs
|-- Services/
|   |-- AnimationEngineTests.cs
|   |-- EasingTests.cs
|   |-- RoomSceneAssignmentServiceTests.cs
|   |-- SceneStorageServiceTests.cs
|   `-- SettingsServiceTests.cs
`-- ViewModels/
    `-- SceneBuilderViewModelTests.cs
```

## Test Structure

**Suite Organization:**
```csharp
// From src/HueCompanion.Tests/Models/ResultTests.cs
namespace HueCompanion.Tests.Models;

public class ResultTests
{
    public class GenericResult
    {
        [Fact]
        public void Success_CreatesSuccessfulResult()
        {
            // Arrange & Act
            var result = Result<int>.Success(42);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42);
        }
    }

    public class NonGenericResult
    {
        [Fact]
        public void Success_CreatesSuccessfulResult()
        {
            // Arrange & Act
            var result = Result.Success();

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}
```

**Patterns:**
- Setup: Constructor (`public TestClassName()`) initializes test data and services
- Teardown: `IDisposable` with `Dispose()` cleans up resources (temp directories, files)
- Assertions: FluentAssertions method chaining (`Should().Be()`, `Should().BeTrue()`, `Should().HaveCount()`)

**Arrange-Act-Assert (AAA):**
```csharp
// From src/HueCompanion.Tests/Models/HueColorTests.cs:33-42
[Fact]
public void White_ReturnsD65WhitePoint()
{
    // Arrange & Act
    var white = HueColor.White;

    // Assert
    white.X.Should().BeApproximately(0.3127, 0.0001);
    white.Y.Should().BeApproximately(0.3290, 0.0001);
}
```

## Mocking

**Framework:** Moq 4.21.0

**Patterns:**
- Mock service interfaces with `Mock<T>` and verify calls with `Setup` / `Verify`.
- Specify every argument in Moq expression trees, including optional parameters. Omitting the optional transition argument causes CS0854.
- Example from `src/HueCompanion.Tests/Services/AnimationEngineTests.cs`:
```csharp
_bridgeMock.Verify(b =>
    b.SetLightColorAndBrightnessAsync(
        lightId, It.IsAny<HueColor>(), It.IsAny<double>(), It.IsAny<int?>()),
    Times.AtLeastOnce());
```

**What to Mock:**
- External API calls (bridge services)
- Long-running operations in unit tests

**File I/O:** Use real files in unique temporary directories for storage and settings tests; inject the directory through the service constructor.

**What NOT to Mock:**
- Core business logic (Result type, HueColor conversions)
- Data transformation logic
- Validation logic
- Model properties

## Fixtures and Factories

**Test Data:**
Factory methods create test data with defaults:
```csharp
// From src/HueCompanion.Tests/Services/SceneStorageServiceTests.cs:350-361
private static AnimatedSceneModel CreateValidScene(string id, string name)
{
    return new AnimatedSceneModel
    {
        Id = id,
        Name = name,
        Animations = new List<AnimationDefinition>
        {
            CreateValidKeyframeAnimation($"{id}-anim-1")
        }
    };
}

private static AnimationDefinition CreateValidKeyframeAnimation(string id)
{
    return new AnimationDefinition
    {
        Id = id,
        Name = "Test Animation",
        Type = AnimationType.Keyframe,
        DurationSeconds = 10,
        Keyframes = new List<AnimationKeyframe>
        {
            new AnimationKeyframe { TimeSeconds = 0, Brightness = 1.0, Color = HueColor.White },
            new AnimationKeyframe { TimeSeconds = 10, Brightness = 0.5, Color = new HueColor(0.5, 0.4) }
        }
    };
}
```

**Location:**
- Helper methods as private static in test class (`CreateValidScene`, `CreateValidKeyframeAnimation`)
- Temp directories created per-test using unique GUID: `Path.Combine(Path.GetTempPath(), $"HueCompanionSceneTest_{Guid.NewGuid()}")`

## Coverage

**Requirements:** No minimum coverage threshold is enforced. Main CI collects coverage with `coverlet.collector`, uploads Cobertura reports to Codecov, and retains TRX results in the `test-results` artifact.

**View Coverage:**
```bash
dotnet test src/HueCompanion.Tests/HueCompanion.Tests.csproj --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

## Test Types

**Unit Tests:**
- Scope: Single method or small logical unit
- Approach: Isolate external dependencies with service mocks; use real models and transformation logic.
- Location: `Models/`, `Services/`, `Mcp/`, and `ViewModels/`.
- Example: `HueColorTests.FromRgb_RoundTripsApproximately` tests color space conversion without bridge

**Integration Tests:**
- Scope: Multiple components working together
- Approach: Construct real services with a unique temporary directory; clean it up with `IDisposable`.
- Location: `SceneStorageServiceTests`, `SettingsServiceTests`
- Example: `SaveSceneAsync_ThenLoadSceneAsync_RoundTripsData` writes to temp disk, reads back

**E2E Tests:**
- No dedicated E2E test framework. `tools/Run-UITests.ps1` launches the Windows app and captures page screenshots; `tools/Compare-Screenshots.ps1` compares them against visual baselines. Main CI runs both with `continue-on-error: true`.
- Manual testing used for full application flow (bridge connection, scene playback)

## Common Patterns

**Async Testing:**
```csharp
// From src/HueCompanion.Tests/Services/SceneStorageServiceTests.cs:30-43
[Fact]
public async Task SaveSceneAsync_CreatesJsonFile()
{
    // Arrange
    var scene = CreateValidScene("test-scene-1", "Test Scene");

    // Act
    var result = await _service.SaveSceneAsync(scene);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var expectedPath = Path.Combine(_testDirectory, "test-scene-1.json");
    File.Exists(expectedPath).Should().BeTrue();
}
```

**xUnit v3 Cancellation:**
- Pass `TestContext.Current.CancellationToken` to test delays and asynchronous file writes that accept cancellation tokens. This resolves analyzer warning xUnit1051 and allows the runner to cancel those operations.
- Keep test methods that perform asynchronous work as `async Task` and await their operations.
```csharp
await Task.Delay(300, TestContext.Current.CancellationToken);
await File.WriteAllTextAsync(
    invalidPath, "{ not valid json }", TestContext.Current.CancellationToken);
```

**Error Testing:**
```csharp
// From src/HueCompanion.Tests/Services/SceneStorageServiceTests.cs:80-91
[Fact]
public async Task LoadSceneAsync_WithNonExistentFile_ReturnsFailure()
{
    // Arrange
    var nonExistentPath = Path.Combine(_testDirectory, "non-existent-scene.json");

    // Act
    var result = await _service.LoadSceneAsync(nonExistentPath);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Contain("not found");
}
```

**Theory Testing (Parameterized):**
```csharp
// From src/HueCompanion.Tests/Models/HueColorTests.cs:55-81
[Theory]
[InlineData(255, 0, 0)]   // Red
[InlineData(0, 255, 0)]   // Green
[InlineData(0, 0, 255)]   // Blue
[InlineData(255, 255, 255)] // White
[InlineData(0, 0, 0)]     // Black -> should return white point
public void FromRgb_RoundTripsApproximately(byte r, byte g, byte b)
{
    // Test implementation
}
```

**Approximate/Tolerance Assertions:**
```csharp
// From src/HueCompanion.Tests/Models/HueColorTests.cs:40-41
white.X.Should().BeApproximately(0.3127, 0.0001);
white.Y.Should().BeApproximately(0.3290, 0.0001);

// From src/HueCompanion.Tests/Models/HueColorTests.cs:77-79
resultR.Should().BeCloseTo(r, 30); // Allow 30 unit tolerance for conversion
resultG.Should().BeCloseTo(g, 30);
resultB.Should().BeCloseTo(b, 30);
```

**Test Cleanup:**
```csharp
// From src/HueCompanion.Tests/Services/SceneStorageServiceTests.cs:8-28
public class SceneStorageServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly SceneStorageService _service;

    public SceneStorageServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"HueCompanionSceneTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _service = new SceneStorageService(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
}
```

## Package Configuration

Dependencies in `src/HueCompanion.Tests/HueCompanion.Tests.csproj`:
- `Microsoft.NET.Test.Sdk` 18.11.0 - Test runner infrastructure
- `xunit.v3` 4.0.2 - Test framework
- `xunit.runner.visualstudio` 4.0.1 - Visual Studio integration
- `Moq` 4.21.0 - Mocking library
- `FluentAssertions` 8.11.0 - Assertion library
- `coverlet.collector` 10.1.0 - Code coverage collection

## Migration Verification and Gaps

- After the dependency migration and Moq/cancellation fixes, `dotnet test src/HueCompanion.Tests/HueCompanion.Tests.csproj --no-restore` passed all 181 tests with 0 failures and 0 skipped tests. The same suite passed after the synchronous scene-loading refactor in `SceneBuilderViewModel`.
- `LoadSceneFromModel` now runs synchronously; `ImportFromJsonAsync` retains its `Task<Result>` API using completed tasks. Exercise scene loading/importing manually when verifying editor behavior.
- The x64 app build and the Windows App SDK's `VerifyLaunchSettings` target passed after setting the launch profile's `commandName` to `MsixPackage` for packaged debugging. These checks do not establish that deployment or launching succeeds.
- Passing unit tests do not validate `HueBridgeService` HTTP payloads or physical bridge behavior. The migrated service now uses `EffectsV2.Action`, explicit power-on dimming/color modes, and an empty `Children` list for room creation. These request shapes still need regression coverage and verification against a real bridge.
- Existing compiler and MVVM Toolkit warnings outside the migration fixes remain; distinguish these from xUnit v3 analyzer warnings.

---

*Testing analysis: 2026-11-07*

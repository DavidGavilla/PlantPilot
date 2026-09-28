# Testing — .NET 8 / xUnit 2.5.3 / SQL Server

Adapted from `codewithmukesh/dotnet-claude-kit`, skill `testing` (MIT License, © 2025 Mukesh Murugan, https://github.com/codewithmukesh/dotnet-claude-kit). Reused: the core principles, `WebApplicationFactory` pattern, Testcontainers approach, test-data-builder pattern, `FakeTimeProvider` pattern, naming convention, and anti-patterns list. **Changed for this repo, per explicit instruction not to force an external example's stack**: the source targets **xUnit v3** and **PostgreSQL** on **.NET 10** — this project is **xUnit 2.5.3**, **SQL Server**, **.NET 8** (`BackEnd/PlantCare.Api.Tests.csproj`), so every code sample below is rewritten for those versions, not copied verbatim.

None of the packages below (`Testcontainers.MsSql`, `Microsoft.AspNetCore.Mvc.Testing`, `WireMock.Net`, `Verify.Xunit`, `Microsoft.Extensions.TimeProvider.Testing`) are installed yet. Add only the one a specific feature actually needs, via `dotnet-implementer`, when the test that needs it is being written — don't bulk-add all of them speculatively.

## Core principles

1. Integration tests via `WebApplicationFactory` are the highest-value single test — they cover routing, binding, validation, service logic, and persistence together.
2. Real databases in integration tests — `Testcontainers.MsSql`, never `UseInMemoryDatabase`. This codebase has already been bitten by InMemory-shaped bugs the real SQL Server provider catches (multi-cascade-path FK rejections, decimal precision truncation, PK-convention misses) — InMemory would hide all of them.
3. AAA (Arrange/Act/Assert) always, matching the existing style in `PlantCare.Api.Tests/Validators/`.
4. Test behavior, not implementation — assert on the observable outcome (DB state, HTTP response), not that a specific internal method was called.

## xUnit v2 basics (this repo's actual version)

```csharp
public class DeviceServiceTests
{
    [Fact]
    public async Task CreateDeviceAsync_WithValidDto_PersistsDeviceForOwningUser()
    {
        // Arrange
        var db = CreateInMemoryDbForUnitLogicOnly(); // OK for pure unit tests with no relational behavior under test
        var service = new DeviceService(db);
        var dto = new CreateDeviceDto { Name = "Soil sensor", DeviceType = DeviceType.MoistureSensor };

        // Act
        var result = await service.CreateDeviceAsync(userId: 1, dto);

        // Assert
        Assert.Equal(1, result.UserId);
        Assert.Equal("Soil sensor", result.Name);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Should_Have_Error_When_Probability_Is_Out_Of_Range(decimal probability) { /* matches existing validator test style */ }
}
```

`InMemoryDatabase` is acceptable **only** for a pure unit test of business logic that has no relational/provider-specific behavior under test (e.g. a mapper or a simple compute path) — anything touching cascades, constraints, precision, or concurrency must use the real SQL Server container below.

## Integration tests with `WebApplicationFactory` (xUnit v2 + SQL Server)

**Precondition**: `Program.cs` currently ends with `app.Run();` and no explicit `Program` class — top-level statements auto-generate an `internal` `Program` class, which `WebApplicationFactory<Program>` cannot see from the test project. Add, once, at the end of `Program.cs`:
```csharp
public partial class Program { }
```

```csharp
// Fixtures/ApiFixture.cs
public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _mssql = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest") // same image as BackEnd/docker-compose.yml
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_mssql.GetConnectionString()));
        });
    }

    // xUnit v2: IAsyncLifetime.InitializeAsync/DisposeAsync return Task, NOT ValueTask (that's xUnit v3)
    public async Task InitializeAsync()
    {
        await _mssql.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(); // applies the real Migrations/ — exercises them too
    }

    public new async Task DisposeAsync()
    {
        await _mssql.DisposeAsync();
    }
}
```

```csharp
// Tests/Devices/CreateDeviceTests.cs
public class CreateDeviceTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task CreateDevice_ReturnsCreated_ForOwningUser()
    {
        // Arrange
        var dto = new CreateDeviceDto { Name = "Soil sensor", DeviceType = DeviceType.MoistureSensor };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/1/devices", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetDevice_ForAnotherUsersDevice_ReturnsNotFound()
    {
        // Arrange — seed a device owned by user 2

        // Act — request it as user 1
        var response = await _client.GetAsync("/api/users/1/devices/{deviceIdOwnedByUser2}");

        // Assert — negative/isolation case, required per the acceptance-criteria table rules
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

## Test data builders

```csharp
public class CreateDeviceDtoBuilder
{
    private string _name = "Test Device";
    private DeviceType _type = DeviceType.MoistureSensor;

    public CreateDeviceDtoBuilder WithName(string name) { _name = name; return this; }
    public CreateDeviceDtoBuilder WithType(DeviceType type) { _type = type; return this; }
    public CreateDeviceDto Build() => new() { Name = _name, DeviceType = _type };
}
```

## Time-dependent code

`TimeProvider` is built into .NET 8. Use `Microsoft.Extensions.TimeProvider.Testing`'s `FakeTimeProvider` for anything asserting on `DateTime.UtcNow`-style fields (e.g. `Device.DateAdded`, `DeviceCommand.DateCreated`) once a service takes `TimeProvider` as a dependency instead of calling `DateTime.UtcNow` directly.

## Naming convention

- FluentValidation validator tests: keep the existing repo convention, `Should_Have_Error_When_X`/`Should_Not_Have_Error_When_X` (see `PlantCare.Api.Tests/Validators/`).
- Everything else (services, integration): `MethodName_StateUnderTest_ExpectedBehavior`, e.g. `CreateDevice_ReturnsCreated_ForOwningUser`, `GetDevice_ForAnotherUsersDevice_ReturnsNotFound`.

## Anti-patterns

- **`UseInMemoryDatabase` for integration tests** — hides real SQL Server behavior; use `Testcontainers.MsSql`.
- **Asserting a mock was called instead of the observable outcome** — assert DB state / HTTP response, not `mock.Verify(...)`.
- **Shared mutable `AppDbContext`/static state across tests** — fresh context or scoped fixture per test.
- **Assertion-free tests** ("it didn't throw, so it works") — always assert the expected outcome.
- **Copying a test pattern from this file (or any external source) without confirming it compiles against this repo's actual types and passes** — every code block above is illustrative; verify field/method names against the real `Models`/`DTOs`/`Services` before using it.

## Decision guide

| Scenario | Recommendation |
|---|---|
| API endpoint behavior | `WebApplicationFactory` integration test |
| Isolated business rule | Unit test with fakes, `InMemoryDatabase` OK if no relational behavior involved |
| Anything relational (constraints, cascades, precision, concurrency) | `Testcontainers.MsSql`, never InMemory |
| Cross-user/isolation check | Integration test — negative case, per `requirements-traceability.md` |
| Time-dependent logic | `TimeProvider` + `FakeTimeProvider` |
| External API dependency (Gemini/AI diagnosis) | `WireMock.Net` or an `HttpMessageHandler` stub — never call the real external API from a test |
| Blazor component (only if/when one exists) | `bUnit`, only if it earns its keep for that component |

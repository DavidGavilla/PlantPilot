# Graph Report - BackEnd  (2026-09-28)

## Corpus Check
- 142 files · ~28,611 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 750 nodes · 1566 edges · 39 communities
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 44 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a7cf9bec`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- DeviceChannel
- DeviceDto
- Diagnosis
- PlantCare.Api.DTOs.Schedules
- http
- Plant
- PlantCare.Api.DTOs.Users
- PlantCare.Api.Models.Devices
- PlantCare.Api.Migrations
- PlantCare.Api.csproj
- AppDbContext
- .GetWorkspaces
- AuthController
- DeviceValidator
- .AddMemberAsync
- .EnsureCallerIsUser
- RefreshTokenTests
- Workspace
- PlantDevice
- PlantCare.Api.DTOs.Devices
- AbstractValidator
- ApiFixture
- .UpdateDevice
- .AttachSessionCookie
- Device
- IDeviceService
- .CreatePlant_WithPlotFromAnotherWorkspace_ReturnsBadRequest

## God Nodes (most connected - your core abstractions)
1. `AppDbContext` - 44 edges
2. `PlantCare.Api.Data` - 22 edges
3. `PlantCare.Api.Models.Devices` - 22 edges
4. `Plant` - 22 edges
5. `PlantServiceResult` - 19 edges
6. `PlantCare.Api.Models.Workspaces` - 18 edges
7. `PlantCare.Api.DTOs.Devices` - 17 edges
8. `DeviceDto` - 16 edges
9. `RefreshTokenTests` - 15 edges
10. `ApiFixture` - 15 edges

## Surprising Connections (you probably didn't know these)
- `ApiFixture` --references--> `Program`  [EXTRACTED]
  PlantCare.Api.Tests/Fixtures/ApiFixture.cs → PlantCare.Api/Program.cs
- `CreateDiagnosisProblemDtoValidatorTests` --references--> `CreateDiagnosisProblemDtoValidator`  [EXTRACTED]
  PlantCare.Api.Tests/Validators/Diagnoses/CreateDiagnosisDtoValidatorTest.cs → PlantCare.Api/Validators/Diagnoses/CreateDiagnosisProblemDtoValidator.cs
- `CreateScheduleDtoValidatorTests` --references--> `CreateScheduleDtoValidator`  [EXTRACTED]
  PlantCare.Api.Tests/Validators/Schedules/CreateScheduleDtoValidatorTests.cs → PlantCare.Api/Validators/Schedules/CreateScheduleDtoValidator.cs
- `UpdateScheduleTaskDtoValidatorTests` --references--> `UpdateScheduleTaskDtoValidator`  [EXTRACTED]
  PlantCare.Api.Tests/Validators/Schedules/UpdateScheduleTaskDtoValidatorTests.cs → PlantCare.Api/Validators/Schedules/UpdateScheduleTaskDtoValidator.cs
- `RefreshTokenTests` --references--> `ApiFixture`  [EXTRACTED]
  PlantCare.Api.Tests/Auth/RefreshTokenTests.cs → PlantCare.Api.Tests/Fixtures/ApiFixture.cs

## Import Cycles
- None detected.

## Communities (39 total, 0 thin omitted)

### Community 0 - "DeviceChannel"
Cohesion: 0.06
Nodes (34): DateTime, CreateDeviceCommandDto, DeviceCommandDto, DateTime, DeviceEventDto, DateTime, CreateDeviceReadingDto, DeviceReadingDto (+26 more)

### Community 1 - "DeviceDto"
Cohesion: 0.23
Nodes (8): HttpPost, DateTime, CreateDeviceDto, DeviceDto, DeviceType, IEnumerable, Task, DeviceService

### Community 2 - "Diagnosis"
Cohesion: 0.07
Nodes (24): PlantCare.Api.Tests.Validators.Diagnoses, PlantCare.Api.DTOs.Diagnoses, PlantCare.Api.Validators.Diagnoses, List, CreateDiagnosisDto, CreateDiagnosisProblemDto, DateTime, List (+16 more)

### Community 3 - "PlantCare.Api.DTOs.Schedules"
Cohesion: 0.06
Nodes (30): PlantCare.Api.Tests.Validators.Schedules, PlantCare.Api.Validators.Schedules, PlantCare.Api.DTOs.Schedules, List, CreateScheduleDto, DateTime, CreateScheduleTaskDto, DateTime (+22 more)

### Community 4 - "http"
Cohesion: 0.07
Nodes (28): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, applicationUrl (+20 more)

### Community 5 - "Plant"
Cohesion: 0.09
Nodes (28): IReadOnlyList, PagedResult, CreatePlantDto, DateTime, PlantDto, UpdatePlantDto, IEnumerable, List (+20 more)

### Community 6 - "PlantCare.Api.DTOs.Users"
Cohesion: 0.05
Nodes (40): PlantCare.Api.DTOs.Users, PlantCare.Api.Validators.Users, Entity, LoginUserDto, RegisterUserDto, UpdateUserDto, UserDto, UserMapper (+32 more)

### Community 7 - "PlantCare.Api.Models.Devices"
Cohesion: 0.06
Nodes (36): PlantCare.Api.Tests.Fixtures, PlantCare.Api.Services.Interfaces.Plants, PlantCare.Api.Models, PlantCare.Api.Services.Plants, PlantCare.Api.Services.Workspaces, PlantCare.Api.Models.Schedules, PlantCare.Api.Services.Interfaces.Workspaces, PlantCare.Api.Services.Auth (+28 more)

### Community 8 - "PlantCare.Api.Migrations"
Cohesion: 0.06
Nodes (20): PlantCare.Api.Migrations, Migration, ModelSnapshot, MigrationBuilder, ModelBuilder, InitialCreate, MigrationBuilder, ModelBuilder (+12 more)

### Community 9 - "PlantCare.Api.csproj"
Cohesion: 0.09
Nodes (20): BCrypt.Net-Next (4.0.3), coverlet.collector (6.0.0), FluentValidation.AspNetCore (11.3.1), Microsoft.AspNetCore.Authentication.JwtBearer (8.0.31), Microsoft.AspNetCore.Mvc.Testing (8.0.16), Microsoft.AspNetCore.OpenApi (8.0.8), Microsoft.EntityFrameworkCore.Design (8.0.16), Microsoft.EntityFrameworkCore.SqlServer (8.0.16) (+12 more)

### Community 10 - "AppDbContext"
Cohesion: 0.11
Nodes (20): PlantCare.Api.Models.Satellite, DbContext, DbSet, ModelBuilder, AppDbContext, DateTime, Geometry, ICollection (+12 more)

### Community 18 - ".GetWorkspaces"
Cohesion: 0.11
Nodes (16): PlantCare.Api.DTOs.Workspaces, ActionResult, CancellationToken, HttpGet, IEnumerable, Task, WorkspacesController, WorkspaceMembershipDto (+8 more)

### Community 24 - "AuthController"
Cohesion: 0.18
Nodes (14): Authorize, ControllerBase, CancellationToken, DateTime, HttpGet, HttpPost, IActionResult, IConfiguration (+6 more)

### Community 25 - "DeviceValidator"
Cohesion: 0.39
Nodes (5): Expression, Func, CreateDeviceDtoValidator, DeviceValidator, UpdateDeviceDtoValidator

### Community 26 - ".AddMemberAsync"
Cohesion: 0.29
Nodes (8): Fact, HttpClient, Task, PlantsDeleteTests, Task, PlantsTestDataSeeder, Fact, Task

### Community 27 - ".EnsureCallerIsUser"
Cohesion: 0.07
Nodes (30): PlantCare.Api.Controllers, PlantCare.Api.Services.Interfaces.Farms, PlantCare.Api.DTOs.Farms, PlantCare.Api.Services.Farms, ActionResult, ApiControllerBase, CancellationToken, HttpGet (+22 more)

### Community 28 - "RefreshTokenTests"
Cohesion: 0.28
Nodes (9): Email, HttpResponseMessage, Password, Fact, HttpClient, string, Task, RefreshTokenTests (+1 more)

### Community 29 - "Workspace"
Cohesion: 0.16
Nodes (11): DateTime, ICollection, Workspace, WorkspaceType, DateTime, User, WorkspaceMember, WorkspaceRole (+3 more)

### Community 30 - "PlantDevice"
Cohesion: 0.32
Nodes (4): CreatePlantDeviceDto, PlantDeviceDto, PlantDeviceMapper, PlantDevice

### Community 31 - "PlantCare.Api.DTOs.Devices"
Cohesion: 0.25
Nodes (5): PlantCare.Api.Mappers.Devices, PlantCare.Api.Services.Device, PlantCare.Api.Services.Interfaces.Device, PlantCare.Api.DTOs.Devices, DeviceEventMapper

### Community 32 - "AbstractValidator"
Cohesion: 0.32
Nodes (5): AbstractValidator, PlantCare.Api.Validators.Devices, CreateDeviceCommandDtoValidator, CreateDeviceReadingDtoValidator, CreatePlantDeviceDtoValidator

### Community 33 - "ApiFixture"
Cohesion: 0.17
Nodes (10): IAsyncLifetime, IClassFixture, IWebHostBuilder, MsSqlContainer, Program, Task, ApiFixture, HttpClient (+2 more)

### Community 34 - ".UpdateDevice"
Cohesion: 0.26
Nodes (8): ActionResult, HttpDelete, HttpGet, HttpPut, IActionResult, IEnumerable, Task, DevicesController

### Community 35 - ".AttachSessionCookie"
Cohesion: 0.27
Nodes (7): HttpClient, string, AuthTestHelper, Fact, HttpClient, Task, PlantsReadTests

### Community 36 - "Device"
Cohesion: 0.28
Nodes (6): UpdateDeviceDto, DeviceMapper, DateTime, ICollection, List, Device

### Community 37 - "IDeviceService"
Cohesion: 0.43
Nodes (3): IEnumerable, Task, IDeviceService

### Community 38 - ".CreatePlant_WithPlotFromAnotherWorkspace_ReturnsBadRequest"
Cohesion: 0.48
Nodes (4): Fact, HttpClient, Task, PlantsCreateTests

## Knowledge Gaps
- **41 isolated node(s):** `PlantCare.Api.Tests.Auth`, `net8.0`, `coverlet.collector (6.0.0)`, `Microsoft.AspNetCore.Mvc.Testing (8.0.16)`, `Microsoft.NET.Test.Sdk (17.8.0)` (+36 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `AppDbContext` connect `AppDbContext` to `DeviceChannel`, `DeviceDto`, `Diagnosis`, `PlantCare.Api.DTOs.Schedules`, `Device`, `Plant`, `PlantCare.Api.DTOs.Users`, `PlantCare.Api.Models.Devices`, `.GetWorkspaces`, `.AddMemberAsync`, `.EnsureCallerIsUser`, `Workspace`, `PlantDevice`?**
  _High betweenness centrality (0.327) - this node is a cross-community bridge._
- **Why does `PlantCare.Api.Data` connect `PlantCare.Api.Models.Devices` to `PlantCare.Api.Migrations`, `.EnsureCallerIsUser`, `PlantCare.Api.DTOs.Devices`?**
  _High betweenness centrality (0.163) - this node is a cross-community bridge._
- **Why does `AuthService` connect `PlantCare.Api.DTOs.Users` to `AuthController`, `AppDbContext`, `PlantCare.Api.Models.Devices`?**
  _High betweenness centrality (0.059) - this node is a cross-community bridge._
- **What connects `PlantCare.Api.Tests.Auth`, `net8.0`, `coverlet.collector (6.0.0)` to the rest of the system?**
  _41 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `DeviceChannel` be split into smaller, more focused modules?**
  _Cohesion score 0.06382978723404255 - nodes in this community are weakly interconnected._
- **Should `Diagnosis` be split into smaller, more focused modules?**
  _Cohesion score 0.07446808510638298 - nodes in this community are weakly interconnected._
- **Should `PlantCare.Api.DTOs.Schedules` be split into smaller, more focused modules?**
  _Cohesion score 0.05870020964360587 - nodes in this community are weakly interconnected._
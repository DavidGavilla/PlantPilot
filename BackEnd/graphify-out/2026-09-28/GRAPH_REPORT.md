# Graph Report - BackEnd  (2026-09-28)

## Corpus Check
- 136 files · ~24,923 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 680 nodes · 1390 edges · 33 communities
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 39 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a7cf9bec`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- DeviceReading
- DeviceDto
- Diagnosis
- PlantCare.Api.DTOs.Schedules
- http
- Plant
- AbstractValidator
- PlantCare.Api.Models.Devices
- PlantCare.Api.Migrations
- PlantCare.Api.csproj
- DeviceChannel
- .EnsureCallerIsUser
- DeviceCommand
- DeviceValidator
- AppDbContext
- .GetFarms
- PlantPhoto
- DeviceEvent
- PlantDevice
- PlantCare.Api.DTOs.Devices
- PlantCare.Api.Validators.Devices

## God Nodes (most connected - your core abstractions)
1. `AppDbContext` - 42 edges
2. `PlantCare.Api.Models.Devices` - 22 edges
3. `Plant` - 22 edges
4. `PlantCare.Api.Data` - 19 edges
5. `PlantServiceResult` - 19 edges
6. `PlantCare.Api.Models.Workspaces` - 18 edges
7. `PlantCare.Api.DTOs.Devices` - 17 edges
8. `DeviceDto` - 16 edges
9. `Device` - 15 edges
10. `PlantCare.Api.Models.Plants` - 15 edges

## Surprising Connections (you probably didn't know these)
- `ApiFixture` --references--> `Program`  [EXTRACTED]
  PlantCare.Api.Tests/Fixtures/ApiFixture.cs → PlantCare.Api/Program.cs
- `CreateDiagnosisProblemDtoValidatorTests` --references--> `CreateDiagnosisProblemDtoValidator`  [EXTRACTED]
  PlantCare.Api.Tests/Validators/Diagnoses/CreateDiagnosisDtoValidatorTest.cs → PlantCare.Api/Validators/Diagnoses/CreateDiagnosisProblemDtoValidator.cs
- `CreateScheduleDtoValidatorTests` --references--> `CreateScheduleDtoValidator`  [EXTRACTED]
  PlantCare.Api.Tests/Validators/Schedules/CreateScheduleDtoValidatorTests.cs → PlantCare.Api/Validators/Schedules/CreateScheduleDtoValidator.cs
- `UpdateScheduleTaskDtoValidatorTests` --references--> `UpdateScheduleTaskDtoValidator`  [EXTRACTED]
  PlantCare.Api.Tests/Validators/Schedules/UpdateScheduleTaskDtoValidatorTests.cs → PlantCare.Api/Validators/Schedules/UpdateScheduleTaskDtoValidator.cs
- `DevicesController` --inherits--> `ApiControllerBase`  [EXTRACTED]
  PlantCare.Api/Controllers/Devices/DevicesController.cs → PlantCare.Api/Controllers/ApiControllerBase.cs

## Import Cycles
- None detected.

## Communities (33 total, 0 thin omitted)

### Community 0 - "DeviceReading"
Cohesion: 0.23
Nodes (8): DateTime, CreateDeviceReadingDto, DeviceReadingDto, DeviceReadingMapper, DateTime, DeviceReading, ReadingType, ReadingUnit

### Community 1 - "DeviceDto"
Cohesion: 0.09
Nodes (25): ActionResult, HttpDelete, HttpGet, HttpPost, HttpPut, IActionResult, IEnumerable, Task (+17 more)

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
Nodes (26): int, IReadOnlyList, PagedResult, CreatePlantDto, DateTime, PlantDto, UpdatePlantDto, IEnumerable (+18 more)

### Community 6 - "AbstractValidator"
Cohesion: 0.06
Nodes (34): AbstractValidator, Authorize, ControllerBase, PlantCare.Api.DTOs.Users, PlantCare.Api.Validators.Users, CancellationToken, HttpGet, HttpPost (+26 more)

### Community 7 - "PlantCare.Api.Models.Devices"
Cohesion: 0.08
Nodes (28): PlantCare.Api.Tests.Fixtures, PlantCare.Api.Services.Interfaces.Plants, PlantCare.Api.Models, PlantCare.Api.Services.Plants, PlantCare.Api.Services.Workspaces, PlantCare.Api.Models.Schedules, PlantCare.Api.Services.Interfaces.Workspaces, PlantCare.Api.Services.Auth (+20 more)

### Community 8 - "PlantCare.Api.Migrations"
Cohesion: 0.07
Nodes (17): PlantCare.Api.Migrations, Migration, ModelSnapshot, MigrationBuilder, ModelBuilder, InitialCreate, MigrationBuilder, ModelBuilder (+9 more)

### Community 9 - "PlantCare.Api.csproj"
Cohesion: 0.09
Nodes (20): BCrypt.Net-Next (4.0.3), coverlet.collector (6.0.0), FluentValidation.AspNetCore (11.3.1), Microsoft.AspNetCore.Authentication.JwtBearer (8.0.31), Microsoft.AspNetCore.Mvc.Testing (8.0.16), Microsoft.AspNetCore.OpenApi (8.0.8), Microsoft.EntityFrameworkCore.Design (8.0.16), Microsoft.EntityFrameworkCore.SqlServer (8.0.16) (+12 more)

### Community 10 - "DeviceChannel"
Cohesion: 0.06
Nodes (32): PlantCare.Api.Models.Satellite, DateTime, PhotoCaptureSchedule, DateTime, DeviceChannel, DeviceChannelType, DateTime, Geometry (+24 more)

### Community 18 - ".EnsureCallerIsUser"
Cohesion: 0.08
Nodes (27): PlantCare.Api.Controllers, PlantCare.Api.DTOs.Workspaces, ActionResult, ApiControllerBase, CancellationToken, HttpDelete, HttpGet, HttpPost (+19 more)

### Community 24 - "DeviceCommand"
Cohesion: 0.26
Nodes (8): DateTime, CreateDeviceCommandDto, DeviceCommandDto, DeviceCommandMapper, DateTime, DeviceCommand, CommandStatus, DeviceCommandType

### Community 25 - "DeviceValidator"
Cohesion: 0.39
Nodes (5): Expression, Func, CreateDeviceDtoValidator, DeviceValidator, UpdateDeviceDtoValidator

### Community 26 - "AppDbContext"
Cohesion: 0.08
Nodes (39): DbContext, DbSet, IAsyncLifetime, IClassFixture, IWebHostBuilder, MsSqlContainer, ModelBuilder, AppDbContext (+31 more)

### Community 27 - ".GetFarms"
Cohesion: 0.10
Nodes (19): PlantCare.Api.Services.Interfaces.Farms, PlantCare.Api.DTOs.Farms, PlantCare.Api.Services.Farms, CancellationToken, HttpGet, IActionResult, Task, FarmsController (+11 more)

### Community 28 - "PlantPhoto"
Cohesion: 0.18
Nodes (9): PlantCare.Api.Validators.Plants, CreatePlantPhotoDto, DateTime, PlantPhotoMapper, DateTime, PlantPhoto, PlantPhotoSource, CreatePlantPhotoDtoValidator (+1 more)

### Community 29 - "DeviceEvent"
Cohesion: 0.24
Nodes (7): DateTime, DeviceEventDto, DeviceEventMapper, DateTime, DeviceEvent, WaterAmountSource, TriggerType

### Community 30 - "PlantDevice"
Cohesion: 0.24
Nodes (5): CreatePlantDeviceDto, PlantDeviceDto, PlantDeviceMapper, PlantDevice, CreatePlantDeviceDtoValidator

### Community 31 - "PlantCare.Api.DTOs.Devices"
Cohesion: 0.36
Nodes (4): PlantCare.Api.Mappers.Devices, PlantCare.Api.Services.Device, PlantCare.Api.Services.Interfaces.Device, PlantCare.Api.DTOs.Devices

### Community 32 - "PlantCare.Api.Validators.Devices"
Cohesion: 0.40
Nodes (3): PlantCare.Api.Validators.Devices, CreateDeviceCommandDtoValidator, CreateDeviceReadingDtoValidator

## Knowledge Gaps
- **40 isolated node(s):** `net8.0`, `coverlet.collector (6.0.0)`, `Microsoft.AspNetCore.Mvc.Testing (8.0.16)`, `Microsoft.NET.Test.Sdk (17.8.0)`, `Testcontainers.MsSql (4.15.0)` (+35 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `AppDbContext` connect `AppDbContext` to `DeviceReading`, `DeviceDto`, `Diagnosis`, `PlantCare.Api.DTOs.Schedules`, `Plant`, `AbstractValidator`, `PlantCare.Api.Models.Devices`, `DeviceChannel`, `.EnsureCallerIsUser`, `DeviceCommand`, `.GetFarms`, `PlantPhoto`, `DeviceEvent`, `PlantDevice`?**
  _High betweenness centrality (0.322) - this node is a cross-community bridge._
- **Why does `PlantCare.Api.Data` connect `PlantCare.Api.Models.Devices` to `PlantCare.Api.Migrations`, `.GetFarms`, `PlantCare.Api.DTOs.Devices`?**
  _High betweenness centrality (0.134) - this node is a cross-community bridge._
- **Why does `PlantsService` connect `Plant` to `AppDbContext`, `PlantCare.Api.Models.Devices`?**
  _High betweenness centrality (0.061) - this node is a cross-community bridge._
- **What connects `net8.0`, `coverlet.collector (6.0.0)`, `Microsoft.AspNetCore.Mvc.Testing (8.0.16)` to the rest of the system?**
  _40 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `DeviceDto` be split into smaller, more focused modules?**
  _Cohesion score 0.08888888888888889 - nodes in this community are weakly interconnected._
- **Should `Diagnosis` be split into smaller, more focused modules?**
  _Cohesion score 0.07446808510638298 - nodes in this community are weakly interconnected._
- **Should `PlantCare.Api.DTOs.Schedules` be split into smaller, more focused modules?**
  _Cohesion score 0.05870020964360587 - nodes in this community are weakly interconnected._
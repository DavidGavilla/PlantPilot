using PlantCare.Api.Data;
using PlantCare.Api.Models;
using PlantCare.Api.Models.Automation;
using PlantCare.Api.Models.Diagnoses;
using PlantCare.Api.Models.Farms;
using PlantCare.Api.Models.Plants;
using PlantCare.Api.Models.Schedules;
using PlantCare.Api.Models.Workspaces;

namespace PlantCare.Api.Tests.Plants;

// Authorization in this app is entirely EF-query-filtering on real WorkspaceMember rows — there is
// no auth scheme to fake a caller identity with — so every integration test seeds real
// Workspace/WorkspaceMember/Plant/Farm/Plot rows directly via AppDbContext, matching the pattern
// required by testing.md.
public static class PlantsTestDataSeeder
{
    public static async Task<User> CreateUserAsync(AppDbContext db, string label)
    {
        var user = new User
        {
            Name = "Test",
            LastName = label,
            Email = $"{label.ToLowerInvariant()}-{Guid.NewGuid():N}@test.local",
            PasswordHash = "not-a-real-hash"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public static async Task<Workspace> CreateWorkspaceAsync(AppDbContext db, string name = "Test Workspace")
    {
        var workspace = new Workspace { Name = name, Type = WorkspaceType.Business };
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();
        return workspace;
    }

    public static async Task<WorkspaceMember> AddMemberAsync(
        AppDbContext db, int workspaceId, int userId, WorkspaceRole role)
    {
        var member = new WorkspaceMember { WorkspaceId = workspaceId, UserId = userId, Role = role };
        db.WorkspaceMembers.Add(member);
        await db.SaveChangesAsync();
        return member;
    }

    public static async Task<Plant> CreatePlantAsync(
        AppDbContext db, int workspaceId, int userId, string name = "Test Plant", int? plotId = null)
    {
        var plant = new Plant
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            Name = name,
            PlotId = plotId
        };

        db.Plants.Add(plant);
        await db.SaveChangesAsync();
        return plant;
    }

    public static async Task<(Farm Farm, Plot Plot)> CreateFarmWithPlotAsync(
        AppDbContext db, int workspaceId, string farmName = "Test Farm", string plotName = "Test Plot")
    {
        var farm = new Farm { WorkspaceId = workspaceId, Name = farmName };
        db.Farms.Add(farm);
        await db.SaveChangesAsync();

        var plot = new Plot { FarmId = farm.FarmId, Name = plotName };
        db.Plots.Add(plot);
        await db.SaveChangesAsync();

        return (farm, plot);
    }

    public static async Task AddAlertAsync(AppDbContext db, int workspaceId, int plantId)
    {
        var alert = new Alert
        {
            WorkspaceId = workspaceId,
            PlantId = plantId,
            Type = AlertType.PlantHealthIssue,
            Severity = AlertSeverity.Warning,
            Message = "Test alert"
        };

        db.Alerts.Add(alert);
        await db.SaveChangesAsync();
    }

    // Seeds a Schedule whose DiagnosisId points at a diagnosis belonging to a *different* plant's
    // photo — an invariant nothing in the schema enforces — to reproduce the DeletePlantAsync
    // pre-check gap the fallback fix guards against.
    public static async Task AddScheduleWithAnotherPlantsDiagnosisAsync(
        AppDbContext db, int targetPlantId, int otherPlantId)
    {
        var photo = new PlantPhoto
        {
            PlantId = otherPlantId,
            ImageUrl = "https://example.test/photo.jpg",
            Source = PlantPhotoSource.Mobile
        };
        db.PlantPhotos.Add(photo);
        await db.SaveChangesAsync();

        var diagnosis = new Diagnosis
        {
            PlantPhotoId = photo.PlantPhotoId,
            HealthStatus = DiagnosisHealthStatus.Healthy,
            AnalysisStatus = DiagnosisAnalysisStatus.Completed,
            HealthProbability = 0.9m,
            AiProvider = "test-provider",
            Summary = "Looks fine.",
            RawAiResponse = "{}"
        };
        db.Diagnoses.Add(diagnosis);
        await db.SaveChangesAsync();

        var schedule = new Schedule
        {
            PlantId = targetPlantId,
            DiagnosisId = diagnosis.DiagnosisId
        };
        db.Schedules.Add(schedule);
        await db.SaveChangesAsync();
    }
}

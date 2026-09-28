using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PlantCare.Api.Data;
using PlantCare.Api.DTOs.Common;
using PlantCare.Api.DTOs.Plants;
using PlantCare.Api.Models.Workspaces;
using PlantCare.Api.Tests.Fixtures;

namespace PlantCare.Api.Tests.Plants;

public class PlantsDeleteTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task DeletePlant_WithNoDependents_HardDeletes_ThenGetReturnsNotFound()
    {
        // Arrange
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var caller = await PlantsTestDataSeeder.CreateUserAsync(db, "HardDeleteCaller");
        var workspace = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Hard Delete Workspace");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspace.WorkspaceId, caller.UserId, WorkspaceRole.Owner);
        var plant = await PlantsTestDataSeeder.CreatePlantAsync(db, workspace.WorkspaceId, caller.UserId);
        AuthTestHelper.AttachSessionCookie(_client, fixture, caller.UserId);

        var route = $"/api/users/{caller.UserId}/workspaces/{workspace.WorkspaceId}/plants/{plant.PlantId}";

        // Act
        var deleteResponse = await _client.DeleteAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync(route);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeletePlant_WithAnAlert_ArchivesInstead_PlantStillReadableWithIsArchivedTrue()
    {
        // Arrange â€” a dependent Alert blocks the hard-delete fast path, so the plant is archived.
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var caller = await PlantsTestDataSeeder.CreateUserAsync(db, "ArchiveCaller");
        var workspace = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Archive Workspace");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspace.WorkspaceId, caller.UserId, WorkspaceRole.Owner);
        var plant = await PlantsTestDataSeeder.CreatePlantAsync(db, workspace.WorkspaceId, caller.UserId, "Alerted Plant");
        await PlantsTestDataSeeder.AddAlertAsync(db, workspace.WorkspaceId, plant.PlantId);
        AuthTestHelper.AttachSessionCookie(_client, fixture, caller.UserId);

        var plantRoute = $"/api/users/{caller.UserId}/workspaces/{workspace.WorkspaceId}/plants/{plant.PlantId}";
        var listRoute = $"/api/users/{caller.UserId}/workspaces/{workspace.WorkspaceId}/plants";

        // Act
        var deleteResponse = await _client.DeleteAsync(plantRoute);

        // Assert â€” archived, not deleted.
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync(plantRoute);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var archived = await getResponse.Content.ReadFromJsonAsync<PlantDto>();
        Assert.NotNull(archived);
        Assert.True(archived!.IsArchived);
        Assert.NotNull(archived.ArchivedAt);

        var includeArchivedResponse = await _client.GetAsync($"{listRoute}?includeArchived=true");
        var includeArchivedResult = await includeArchivedResponse.Content.ReadFromJsonAsync<PagedResult<PlantDto>>();
        Assert.Contains(includeArchivedResult!.Items, p => p.PlantId == plant.PlantId);

        var excludeArchivedResponse = await _client.GetAsync($"{listRoute}?includeArchived=false");
        var excludeArchivedResult = await excludeArchivedResponse.Content.ReadFromJsonAsync<PagedResult<PlantDto>>();
        Assert.DoesNotContain(excludeArchivedResult!.Items, p => p.PlantId == plant.PlantId);
    }

    [Fact]
    public async Task DeletePlant_AlreadyArchived_IsIdempotent_ReturnsSuccess()
    {
        // Arrange â€” archive it once via a dependent Alert, then delete again.
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var caller = await PlantsTestDataSeeder.CreateUserAsync(db, "IdempotentCaller");
        var workspace = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Idempotent Workspace");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspace.WorkspaceId, caller.UserId, WorkspaceRole.Owner);
        var plant = await PlantsTestDataSeeder.CreatePlantAsync(db, workspace.WorkspaceId, caller.UserId);
        await PlantsTestDataSeeder.AddAlertAsync(db, workspace.WorkspaceId, plant.PlantId);
        AuthTestHelper.AttachSessionCookie(_client, fixture, caller.UserId);

        var route = $"/api/users/{caller.UserId}/workspaces/{workspace.WorkspaceId}/plants/{plant.PlantId}";

        var firstDelete = await _client.DeleteAsync(route);
        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);

        // Act â€” repeat DELETE on the now-archived plant.
        var secondDelete = await _client.DeleteAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }

    [Fact]
    public async Task DeletePlant_WithScheduleReferencingAnotherPlantsDiagnosis_FallsBackToArchive()
    {
        // Arrange â€” reproduces the gap the database-reviewer flagged: a Schedule row whose
        // DiagnosisId belongs to a *different* plant's photo. The pre-check sees DiagnosisId != null
        // and treats it as "safe" (reachable via the Photo -> Diagnosis -> Schedule cascade), but for
        // this plant it isn't, so SQL Server's direct Plant -> Schedule Restrict FK rejects the hard
        // delete. Confirms TryHardDeleteAsync catches that and falls back to archiving instead of an
        // unhandled 500.
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var caller = await PlantsTestDataSeeder.CreateUserAsync(db, "FallbackCaller");
        var workspace = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Fallback Workspace");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspace.WorkspaceId, caller.UserId, WorkspaceRole.Owner);

        var targetPlant = await PlantsTestDataSeeder.CreatePlantAsync(db, workspace.WorkspaceId, caller.UserId, "Target Plant");
        var otherPlant = await PlantsTestDataSeeder.CreatePlantAsync(db, workspace.WorkspaceId, caller.UserId, "Other Plant");

        await PlantsTestDataSeeder.AddScheduleWithAnotherPlantsDiagnosisAsync(db, targetPlant.PlantId, otherPlant.PlantId);
        AuthTestHelper.AttachSessionCookie(_client, fixture, caller.UserId);

        var route = $"/api/users/{caller.UserId}/workspaces/{workspace.WorkspaceId}/plants/{targetPlant.PlantId}";

        // Act
        var deleteResponse = await _client.DeleteAsync(route);

        // Assert â€” no unhandled 500; the plant survives, archived.
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync(route);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var result = await getResponse.Content.ReadFromJsonAsync<PlantDto>();
        Assert.NotNull(result);
        Assert.True(result!.IsArchived);
    }
}

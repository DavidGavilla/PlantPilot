using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PlantCare.Api.Data;
using PlantCare.Api.DTOs.Common;
using PlantCare.Api.DTOs.Plants;
using PlantCare.Api.Models.Workspaces;
using PlantCare.Api.Tests.Fixtures;

namespace PlantCare.Api.Tests.Plants;

public class PlantsReadTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task GetPlant_ForPlantInAnotherWorkspace_ReturnsNotFound()
    {
        // Arrange â€” caller is a real member of workspace A, but the plant lives in workspace B.
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var caller = await PlantsTestDataSeeder.CreateUserAsync(db, "Caller");
        var workspaceA = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Workspace A");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspaceA.WorkspaceId, caller.UserId, WorkspaceRole.Owner);

        var otherOwner = await PlantsTestDataSeeder.CreateUserAsync(db, "OtherOwner");
        var workspaceB = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Workspace B");
        var plantInB = await PlantsTestDataSeeder.CreatePlantAsync(db, workspaceB.WorkspaceId, otherOwner.UserId);
        AuthTestHelper.AttachSessionCookie(_client, fixture, caller.UserId);

        // Act â€” request the workspace-B plant through workspace A's route.
        var response = await _client.GetAsync(
            $"/api/users/{caller.UserId}/workspaces/{workspaceA.WorkspaceId}/plants/{plantInB.PlantId}");

        // Assert â€” cross-tenant isolation: no existence leak, just a plain 404.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPlants_ForNonMember_ReturnsForbidden()
    {
        // Arrange â€” a real workspace exists, but the caller has no WorkspaceMember row for it.
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var nonMember = await PlantsTestDataSeeder.CreateUserAsync(db, "NonMember");
        var workspace = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "No Access Workspace");
        AuthTestHelper.AttachSessionCookie(_client, fixture, nonMember.UserId);

        // Act
        var response = await _client.GetAsync(
            $"/api/users/{nonMember.UserId}/workspaces/{workspace.WorkspaceId}/plants");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPlants_WithInvalidPagination_ClampsToDefaults()
    {
        // Arrange
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var caller = await PlantsTestDataSeeder.CreateUserAsync(db, "PagingCaller");
        var workspace = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Paging Workspace");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspace.WorkspaceId, caller.UserId, WorkspaceRole.Owner);
        await PlantsTestDataSeeder.CreatePlantAsync(db, workspace.WorkspaceId, caller.UserId, "Plant 1");
        AuthTestHelper.AttachSessionCookie(_client, fixture, caller.UserId);

        // Act â€” zero/negative page and pageSize must not 500 or misbehave.
        var response = await _client.GetAsync(
            $"/api/users/{caller.UserId}/workspaces/{workspace.WorkspaceId}/plants?page=0&pageSize=-5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<PlantDto>>();
        Assert.NotNull(result);
        Assert.Equal(1, result!.Page);
        Assert.Equal(20, result.PageSize);
    }
}

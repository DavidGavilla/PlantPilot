using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PlantCare.Api.Data;
using PlantCare.Api.DTOs.Plants;
using PlantCare.Api.Models.Workspaces;
using PlantCare.Api.Tests.Fixtures;

namespace PlantCare.Api.Tests.Plants;

public class PlantsCreateTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task CreatePlant_ForWorkspaceMember_ReturnsCreated()
    {
        // Arrange â€” any role, including the lowest (Member), can create.
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var caller = await PlantsTestDataSeeder.CreateUserAsync(db, "Creator");
        var workspace = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Create Workspace");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspace.WorkspaceId, caller.UserId, WorkspaceRole.Member);
        AuthTestHelper.AttachSessionCookie(_client, fixture, caller.UserId);

        var dto = new CreatePlantDto { Name = "Monstera" };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/users/{caller.UserId}/workspaces/{workspace.WorkspaceId}/plants", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<PlantDto>();
        Assert.NotNull(created);
        Assert.Equal("Monstera", created!.Name);
        Assert.Equal(workspace.WorkspaceId, created.WorkspaceId);
    }

    [Fact]
    public async Task CreatePlant_WithUserIdInBody_IgnoresIt_OwnerIsAlwaysRouteUserId()
    {
        // Arrange â€” CreatePlantDto has no UserId slot to bind to; an extra "userId" field in the
        // JSON body must be silently ignored by the deserializer, never mass-assigned as owner.
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var caller = await PlantsTestDataSeeder.CreateUserAsync(db, "RouteOwner");
        var workspace = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Mass Assignment Workspace");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspace.WorkspaceId, caller.UserId, WorkspaceRole.Owner);
        AuthTestHelper.AttachSessionCookie(_client, fixture, caller.UserId);

        var payload = new { name = "Fern", userId = 999999 };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/users/{caller.UserId}/workspaces/{workspace.WorkspaceId}/plants", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<PlantDto>();
        Assert.NotNull(created);
        Assert.Equal(caller.UserId, created!.UserId);
        Assert.NotEqual(999999, created.UserId);
    }

    [Fact]
    public async Task CreatePlant_WithPlotFromAnotherWorkspace_ReturnsBadRequest()
    {
        // Arrange â€” the PlotId belongs to a Farm/Plot in a different workspace.
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var caller = await PlantsTestDataSeeder.CreateUserAsync(db, "CrossPlotCaller");
        var workspaceA = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Plot Workspace A");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspaceA.WorkspaceId, caller.UserId, WorkspaceRole.Owner);

        var workspaceB = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Plot Workspace B");
        var (_, plotInB) = await PlantsTestDataSeeder.CreateFarmWithPlotAsync(db, workspaceB.WorkspaceId);
        AuthTestHelper.AttachSessionCookie(_client, fixture, caller.UserId);

        var dto = new CreatePlantDto { Name = "Cross-workspace plant", PlotId = plotInB.PlotId };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/users/{caller.UserId}/workspaces/{workspaceA.WorkspaceId}/plants", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PlantCare.Api.Data;
using PlantCare.Api.DTOs.Plants;
using PlantCare.Api.Models.Workspaces;
using PlantCare.Api.Tests.Fixtures;

namespace PlantCare.Api.Tests.Plants;

public class PlantsUpdateTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task UpdatePlant_AsMember_OnAnotherMembersPlant_ReturnsForbidden()
    {
        // Arrange â€” both users are plain Members; a Member may only touch their own plants.
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var workspace = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Member Restriction Workspace");

        var caller = await PlantsTestDataSeeder.CreateUserAsync(db, "CallerMember");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspace.WorkspaceId, caller.UserId, WorkspaceRole.Member);

        var otherMember = await PlantsTestDataSeeder.CreateUserAsync(db, "OtherMember");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspace.WorkspaceId, otherMember.UserId, WorkspaceRole.Member);
        var plant = await PlantsTestDataSeeder.CreatePlantAsync(db, workspace.WorkspaceId, otherMember.UserId);
        AuthTestHelper.AttachSessionCookie(_client, fixture, caller.UserId);

        var dto = new UpdatePlantDto { Name = "Renamed by non-owner" };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/users/{caller.UserId}/workspaces/{workspace.WorkspaceId}/plants/{plant.PlantId}", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePlant_AsOwner_OnAnyMembersPlant_ReturnsOk()
    {
        // Arrange â€” Owner/Admin roles bypass the ownership restriction.
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var workspace = await PlantsTestDataSeeder.CreateWorkspaceAsync(db, "Owner Bypass Workspace");

        var owner = await PlantsTestDataSeeder.CreateUserAsync(db, "OwnerCaller");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspace.WorkspaceId, owner.UserId, WorkspaceRole.Owner);

        var member = await PlantsTestDataSeeder.CreateUserAsync(db, "PlantOwnerMember");
        await PlantsTestDataSeeder.AddMemberAsync(db, workspace.WorkspaceId, member.UserId, WorkspaceRole.Member);
        var plant = await PlantsTestDataSeeder.CreatePlantAsync(db, workspace.WorkspaceId, member.UserId);
        AuthTestHelper.AttachSessionCookie(_client, fixture, owner.UserId);

        var dto = new UpdatePlantDto { Name = "Renamed by owner" };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/users/{owner.UserId}/workspaces/{workspace.WorkspaceId}/plants/{plant.PlantId}", dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<PlantDto>();
        Assert.NotNull(updated);
        Assert.Equal("Renamed by owner", updated!.Name);
    }
}

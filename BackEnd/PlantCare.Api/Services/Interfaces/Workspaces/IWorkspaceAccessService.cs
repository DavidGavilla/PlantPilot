using PlantCare.Api.Models.Workspaces;

namespace PlantCare.Api.Services.Interfaces.Workspaces;

public interface IWorkspaceAccessService
{
    // Returns null if no WorkspaceMember row exists for (userId, workspaceId) — including if the
    // workspace doesn't exist at all — otherwise the member's WorkspaceRole.
    Task<WorkspaceRole?> GetMembershipRoleAsync(int userId, int workspaceId, CancellationToken cancellationToken = default);
}

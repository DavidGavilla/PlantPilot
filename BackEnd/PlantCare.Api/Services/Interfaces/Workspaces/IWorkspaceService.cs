using PlantCare.Api.DTOs.Workspaces;

namespace PlantCare.Api.Services.Interfaces.Workspaces;

public interface IWorkspaceService
{
    Task<IEnumerable<WorkspaceMembershipDto>> GetWorkspacesForUserAsync(int userId, CancellationToken cancellationToken = default);
}

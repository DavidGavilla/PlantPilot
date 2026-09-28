using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.DTOs.Workspaces;
using PlantCare.Api.Services.Interfaces.Workspaces;

namespace PlantCare.Api.Services.Workspaces;

public class WorkspaceService : IWorkspaceService
{
    private readonly AppDbContext _context;

    public WorkspaceService(AppDbContext context)
    {
        _context = context;
    }

    // Self-scoped to the caller's own userId from the route — no cross-user data is exposed here,
    // so no membership check is needed beyond the WHERE clause itself.
    public async Task<IEnumerable<WorkspaceMembershipDto>> GetWorkspacesForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceMembers
            .Where(m => m.UserId == userId)
            .Select(m => new WorkspaceMembershipDto
            {
                WorkspaceId = m.WorkspaceId,
                Name = m.Workspace.Name,
                Type = m.Workspace.Type,
                Role = m.Role
            })
            .ToListAsync(cancellationToken);
    }
}

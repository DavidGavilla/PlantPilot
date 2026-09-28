using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Models.Workspaces;
using PlantCare.Api.Services.Interfaces.Workspaces;

namespace PlantCare.Api.Services.Workspaces;

public class WorkspaceAccessService : IWorkspaceAccessService
{
    private readonly AppDbContext _context;

    public WorkspaceAccessService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WorkspaceRole?> GetMembershipRoleAsync(int userId, int workspaceId, CancellationToken cancellationToken = default)
    {
        var member = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(m => m.UserId == userId && m.WorkspaceId == workspaceId, cancellationToken);

        return member?.Role;
    }
}

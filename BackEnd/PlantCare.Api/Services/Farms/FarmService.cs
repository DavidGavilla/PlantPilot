using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.DTOs.Farms;
using PlantCare.Api.Services.Interfaces.Farms;
using PlantCare.Api.Services.Interfaces.Workspaces;

namespace PlantCare.Api.Services.Farms;

public class FarmService : IFarmService
{
    private readonly AppDbContext _context;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public FarmService(AppDbContext context, IWorkspaceAccessService workspaceAccessService)
    {
        _context = context;
        _workspaceAccessService = workspaceAccessService;
    }

    public async Task<IEnumerable<FarmWithPlotsDto>?> GetFarmsWithPlotsAsync(int userId, int workspaceId, CancellationToken cancellationToken = default)
    {
        var role = await _workspaceAccessService.GetMembershipRoleAsync(userId, workspaceId, cancellationToken);

        if (role == null)
            return null;

        // Any role can read — this is just a selector list, not a write.
        return await _context.Farms
            .Where(f => f.WorkspaceId == workspaceId)
            .Include(f => f.Plots)
            .Select(f => new FarmWithPlotsDto
            {
                FarmId = f.FarmId,
                Name = f.Name,
                Plots = f.Plots.Select(p => new PlotOptionDto
                {
                    PlotId = p.PlotId,
                    Name = p.Name,
                    CropType = p.CropType
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}

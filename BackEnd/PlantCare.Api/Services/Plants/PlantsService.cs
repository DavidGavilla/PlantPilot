using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.DTOs.Common;
using PlantCare.Api.DTOs.Plants;
using PlantCare.Api.Mappers;
using PlantCare.Api.Models.Plants;
using PlantCare.Api.Models.Schedules;
using PlantCare.Api.Models.Workspaces;
using PlantCare.Api.Services.Interfaces.Plants;
using PlantCare.Api.Services.Interfaces.Workspaces;

namespace PlantCare.Api.Services.Plants;

public class PlantsService : IPlantsService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly AppDbContext _context;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public PlantsService(AppDbContext context, IWorkspaceAccessService workspaceAccessService)
    {
        _context = context;
        _workspaceAccessService = workspaceAccessService;
    }

    public async Task<PlantServiceResult<PagedResult<PlantDto>>> GetPlantsAsync(
        int userId,
        int workspaceId,
        string? search,
        int page,
        int pageSize,
        bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        var role = await _workspaceAccessService.GetMembershipRoleAsync(userId, workspaceId, cancellationToken);
        if (role == null)
            return PlantServiceResult<PagedResult<PlantDto>>.NotAMember();

        // Never trust page/pageSize from the caller — this is a public boundary.
        page = page < 1 ? 1 : page;
        pageSize = pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var query = _context.Plants.AsNoTracking().Where(p => p.WorkspaceId == workspaceId);

        if (!includeArchived)
            query = query.Where(p => !p.IsArchived);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        var totalCount = await query.CountAsync(cancellationToken);

        var plants = await query
            .Include(p => p.Plot)
                .ThenInclude(plot => plot!.Farm)
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<PlantDto>
        {
            Items = plants.Select(PlantMapper.ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return PlantServiceResult<PagedResult<PlantDto>>.Ok(result);
    }

    public async Task<PlantServiceResult<PlantDto>> GetPlantByIdAsync(
        int userId,
        int workspaceId,
        int plantId,
        CancellationToken cancellationToken = default)
    {
        var role = await _workspaceAccessService.GetMembershipRoleAsync(userId, workspaceId, cancellationToken);
        if (role == null)
            return PlantServiceResult<PlantDto>.NotAMember();

        var plant = await FindPlantAsync(workspaceId, plantId, cancellationToken);

        if (plant == null)
            return PlantServiceResult<PlantDto>.NotFound();

        return PlantServiceResult<PlantDto>.Ok(PlantMapper.ToDto(plant));
    }

    public async Task<PlantServiceResult<PlantDto>> CreatePlantAsync(
        int userId,
        int workspaceId,
        CreatePlantDto dto,
        CancellationToken cancellationToken = default)
    {
        var role = await _workspaceAccessService.GetMembershipRoleAsync(userId, workspaceId, cancellationToken);
        if (role == null)
            return PlantServiceResult<PlantDto>.NotAMember();

        if (dto.PlotId.HasValue && !await IsPlotInWorkspaceAsync(dto.PlotId.Value, workspaceId, cancellationToken))
            return PlantServiceResult<PlantDto>.Invalid("PlotId does not belong to this workspace.");

        var plant = PlantMapper.ToModel(dto);
        plant.UserId = userId;
        plant.WorkspaceId = workspaceId;

        _context.Plants.Add(plant);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await FindPlantAsync(workspaceId, plant.PlantId, cancellationToken);

        return PlantServiceResult<PlantDto>.Ok(PlantMapper.ToDto(created!));
    }

    public async Task<PlantServiceResult<PlantDto>> UpdatePlantAsync(
        int userId,
        int workspaceId,
        int plantId,
        UpdatePlantDto dto,
        CancellationToken cancellationToken = default)
    {
        var role = await _workspaceAccessService.GetMembershipRoleAsync(userId, workspaceId, cancellationToken);
        if (role == null)
            return PlantServiceResult<PlantDto>.NotAMember();

        // 404 first, before any role/ownership logic — never leak cross-workspace existence.
        var plant = await _context.Plants
            .FirstOrDefaultAsync(p => p.PlantId == plantId && p.WorkspaceId == workspaceId, cancellationToken);

        if (plant == null)
            return PlantServiceResult<PlantDto>.NotFound();

        if (role == WorkspaceRole.Member && plant.UserId != userId)
            return PlantServiceResult<PlantDto>.Forbidden();

        if (dto.PlotId.HasValue && !await IsPlotInWorkspaceAsync(dto.PlotId.Value, workspaceId, cancellationToken))
            return PlantServiceResult<PlantDto>.Invalid("PlotId does not belong to this workspace.");

        PlantMapper.UpdateModel(plant, dto);
        await _context.SaveChangesAsync(cancellationToken);

        var updated = await FindPlantAsync(workspaceId, plantId, cancellationToken);

        return PlantServiceResult<PlantDto>.Ok(PlantMapper.ToDto(updated!));
    }

    public async Task<PlantServiceResult> DeletePlantAsync(
        int userId,
        int workspaceId,
        int plantId,
        CancellationToken cancellationToken = default)
    {
        var role = await _workspaceAccessService.GetMembershipRoleAsync(userId, workspaceId, cancellationToken);
        if (role == null)
            return PlantServiceResult.NotAMember();

        var plant = await _context.Plants
            .FirstOrDefaultAsync(p => p.PlantId == plantId && p.WorkspaceId == workspaceId, cancellationToken);

        if (plant == null)
            return PlantServiceResult.NotFound();

        if (role == WorkspaceRole.Member && plant.UserId != userId)
            return PlantServiceResult.Forbidden();

        // Idempotent — a repeat DELETE on an already-archived plant is not an error.
        if (plant.IsArchived)
            return PlantServiceResult.Ok();

        var hasAlerts = await _context.Alerts.AnyAsync(a => a.PlantId == plantId, cancellationToken);
        var hasIrrigationLinks = await _context.IrrigationZonePlants.AnyAsync(zp => zp.PlantId == plantId, cancellationToken);
        var hasOrphanSchedules = await _context.Schedules
            .AnyAsync(s => s.PlantId == plantId && s.DiagnosisId == null, cancellationToken);

        if (!hasAlerts && !hasIrrigationLinks && !hasOrphanSchedules)
        {
            // Fast path only — existing cascade FKs clean up Photos/Diagnoses/diagnosis-linked
            // Schedules+Tasks/PlantDevices/PhotoCaptureSchedules automatically in the common case.
            // This pre-check can still be wrong: nothing enforces that a Schedule.DiagnosisId
            // actually belongs to a photo of *this* plant (the Photo -> Diagnosis -> Schedule
            // cascade chain it relies on may be rooted at a different plant), and a concurrent
            // request can insert an Alert/IrrigationZonePlant/orphan Schedule between the checks
            // above and the delete below. Either case surfaces as a DbUpdateException from the
            // direct Plant -> Schedule Restrict FK, which TryHardDeleteAsync catches so we can fall
            // back to archiving instead of letting it bubble up as an unhandled 500.
            if (await TryHardDeleteAsync(plant, cancellationToken))
                return PlantServiceResult.Ok();

            // The failed Remove + rolled-back transaction leaves the change tracker holding `plant`
            // in a stale Deleted entry; detach and reload a fresh tracked instance before falling
            // through to the archive logic below, which mutates and saves it.
            _context.Entry(plant).State = EntityState.Detached;

            var reloaded = await _context.Plants
                .FirstOrDefaultAsync(p => p.PlantId == plantId && p.WorkspaceId == workspaceId, cancellationToken);

            if (reloaded == null)
                return PlantServiceResult.NotFound();

            plant = reloaded;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var scheduleIds = await _context.Schedules
            .Where(s => s.PlantId == plantId)
            .Select(s => s.ScheduleId)
            .ToListAsync(cancellationToken);

        if (scheduleIds.Count > 0)
        {
            await _context.ScheduleTasks
                .Where(t => scheduleIds.Contains(t.ScheduleId) && t.Status == ScheduleTaskStatus.Pending)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.Status, ScheduleTaskStatus.Cancelled)
                    .SetProperty(t => t.CancelledAt, DateTime.UtcNow), cancellationToken);
        }

        await _context.PhotoCaptureSchedules
            .Where(s => s.PlantId == plantId && s.IsActive)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.IsActive, false), cancellationToken);

        await _context.IrrigationZonePlants
            .Where(zp => zp.PlantId == plantId)
            .ExecuteDeleteAsync(cancellationToken);

        plant.IsArchived = true;
        plant.ArchivedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return PlantServiceResult.Ok();
    }

    // Read-only helper: used for the pure GetPlantByIdAsync read, and for the post-mutation reload
    // in CreatePlantAsync/UpdatePlantAsync used only to build the response DTO and then discarded —
    // no caller depends on the returned instance being tracked, so AsNoTracking() is safe here.
    private Task<Plant?> FindPlantAsync(int workspaceId, int plantId, CancellationToken cancellationToken)
    {
        return _context.Plants
            .AsNoTracking()
            .Include(p => p.Plot)
                .ThenInclude(plot => plot!.Farm)
            .FirstOrDefaultAsync(p => p.PlantId == plantId && p.WorkspaceId == workspaceId, cancellationToken);
    }

    private Task<bool> IsPlotInWorkspaceAsync(int plotId, int workspaceId, CancellationToken cancellationToken)
    {
        return _context.Plots.AnyAsync(p => p.PlotId == plotId && p.Farm.WorkspaceId == workspaceId, cancellationToken);
    }

    // Attempts the hard delete inside its own transaction. Returns false (after rolling back) on a
    // DbUpdateException instead of propagating it, so the caller can fall back to archiving.
    private async Task<bool> TryHardDeleteAsync(Plant plant, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _context.Plants.Remove(plant);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }
    }
}

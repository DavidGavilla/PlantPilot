using PlantCare.Api.DTOs.Farms;

namespace PlantCare.Api.Services.Interfaces.Farms;

public interface IFarmService
{
    // Returns null when the caller is not a member of the workspace (sentinel for "not a member" —
    // the controller maps this to 403).
    Task<IEnumerable<FarmWithPlotsDto>?> GetFarmsWithPlotsAsync(int userId, int workspaceId, CancellationToken cancellationToken = default);
}

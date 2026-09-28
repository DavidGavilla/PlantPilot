using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.DTOs.Plants;
using PlantCare.Api.Services.Interfaces.Plants;

namespace PlantCare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:int}/workspaces/{workspaceId:int}/plants")]
public class PlantsController : ApiControllerBase
{
    private readonly IPlantsService _plantsService;

    public PlantsController(IPlantsService plantsService)
    {
        _plantsService = plantsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlants(
        int userId,
        int workspaceId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        if (EnsureCallerIsUser(userId) is { } denied) return denied;

        var result = await _plantsService.GetPlantsAsync(userId, workspaceId, search, page, pageSize, includeArchived, cancellationToken);

        return MapResult(result);
    }

    [HttpGet("{plantId:int}")]
    public async Task<IActionResult> GetPlant(int userId, int workspaceId, int plantId, CancellationToken cancellationToken)
    {
        if (EnsureCallerIsUser(userId) is { } denied) return denied;

        var result = await _plantsService.GetPlantByIdAsync(userId, workspaceId, plantId, cancellationToken);

        return MapResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlant(int userId, int workspaceId, CreatePlantDto dto, CancellationToken cancellationToken)
    {
        if (EnsureCallerIsUser(userId) is { } denied) return denied;

        var result = await _plantsService.CreatePlantAsync(userId, workspaceId, dto, cancellationToken);

        if (result.Status == PlantServiceStatus.Success)
        {
            return CreatedAtAction(
                nameof(GetPlant),
                new { userId, workspaceId, plantId = result.Value!.PlantId },
                result.Value
            );
        }

        return MapResult(result);
    }

    [HttpPut("{plantId:int}")]
    public async Task<IActionResult> UpdatePlant(int userId, int workspaceId, int plantId, UpdatePlantDto dto, CancellationToken cancellationToken)
    {
        if (EnsureCallerIsUser(userId) is { } denied) return denied;

        var result = await _plantsService.UpdatePlantAsync(userId, workspaceId, plantId, dto, cancellationToken);

        return MapResult(result);
    }

    [HttpDelete("{plantId:int}")]
    public async Task<IActionResult> DeletePlant(int userId, int workspaceId, int plantId, CancellationToken cancellationToken)
    {
        if (EnsureCallerIsUser(userId) is { } denied) return denied;

        var result = await _plantsService.DeletePlantAsync(userId, workspaceId, plantId, cancellationToken);

        return MapResult(result);
    }

    private IActionResult MapResult<T>(PlantServiceResult<T> result)
    {
        return result.Status switch
        {
            PlantServiceStatus.Success => Ok(result.Value),
            PlantServiceStatus.ValidationError => BadRequest(result.ErrorMessage),
            _ => MapNonSuccess(result)
        };
    }

    private IActionResult MapResult(PlantServiceResult result)
    {
        return result.Status switch
        {
            PlantServiceStatus.Success => NoContent(),
            _ => MapNonSuccess(result)
        };
    }

    // This app has no auth scheme, so Forbid()'s challenge-based behavior isn't appropriate — a
    // plain 403 ObjectResult is used instead (same pattern as FarmsController).
    private IActionResult MapNonSuccess(PlantServiceResult result)
    {
        return result.Status switch
        {
            PlantServiceStatus.NotAMember => StatusCode(403, "Not a member of this workspace."),
            PlantServiceStatus.NotFound => NotFound("Plant not found."),
            PlantServiceStatus.Forbidden => StatusCode(403, "Not allowed to modify this plant."),
            _ => StatusCode(500, "Unexpected error.")
        };
    }
}

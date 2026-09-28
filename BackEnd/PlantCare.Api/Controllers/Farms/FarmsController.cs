using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Services.Interfaces.Farms;

namespace PlantCare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:int}/workspaces/{workspaceId:int}/farms")]
public class FarmsController : ApiControllerBase
{
    private readonly IFarmService _farmService;

    public FarmsController(IFarmService farmService)
    {
        _farmService = farmService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFarms(int userId, int workspaceId, CancellationToken cancellationToken)
    {
        if (EnsureCallerIsUser(userId) is { } denied) return denied;

        var farms = await _farmService.GetFarmsWithPlotsAsync(userId, workspaceId, cancellationToken);

        // This app has no auth scheme, so Forbid()'s challenge-based behavior isn't appropriate —
        // a plain 403 ObjectResult is used instead.
        if (farms == null)
            return StatusCode(403, "Not a member of this workspace.");

        return Ok(farms);
    }
}
